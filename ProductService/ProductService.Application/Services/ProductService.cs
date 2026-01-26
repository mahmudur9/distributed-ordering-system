using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using ProductService.Application.IServices;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Constants;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IHttpClientFactory httpClientFactory)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PaginatedResponse<ProductResponse>> GetAllProductsAsync(GetAllProductsFilter filter)
    {
        try
        {
            List<Expression<Func<Product, bool>>> filters = [];
            filters.Add(x => x.IsActive == filter.IsActive);
            if (filter.Name is not null) filters.Add(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            
            var products = await _unitOfWork.ProductRepository.GetAllAsync(filters, [x => x.Category!, x => x.Pictures], 
                filter.ItemsPerPage, filter.PageNumber, x => x.OrderByDescending(o => o.CreatedAt));
            
            var productCount = await _unitOfWork.ProductRepository.CountAsync(filters);

            var paginatedResponse = new PaginatedResponse<ProductResponse>(
                _mapper.Map<IEnumerable<ProductResponse>>(products),
                productCount,
                filter.ItemsPerPage, 
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            return _mapper.Map<ProductResponse>(product);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void ValidatePictures(List<PictureRequest> picturesRequest)
    {
        if (picturesRequest.Count == 0)
        {
            throw new ArgumentException("Product picture is required!");
        }

        if (picturesRequest.Count > 5)
        {
            throw new  ArgumentException("You are not allowed to upload more than five pictures!");
        }
        foreach (var picture in picturesRequest)
        {
            if (!(picture.Type == Constants.PictureFromLink || picture.Type == Constants.PictureFromFile))
            {
                throw new ArgumentException("Invalid picture type!");
            }

            if (picture.Type == Constants.PictureFromLink && string.IsNullOrEmpty(picture.Url))
            {
                throw new ArgumentException("The picture filed not be empty!");
            }

            if (picture.Type == Constants.PictureFromFile && picture.MediaFile is null)
            {
                throw new ArgumentException("The picture filed not be empty!");
            }
        }
    }

    private async Task UploadPicturesAsync(List<PictureRequest> picturesRequest, List<Picture> pictures)
    {
        pictures.Clear();
            
        foreach (var picture in picturesRequest)
        {
            if (picture.Type == Constants.PictureFromLink)
            {
                pictures.Add(new Picture()
                {
                    Type = Constants.PictureFromLink,
                    Url = picture.Url!
                });
            }
            else if (picture.Type == Constants.PictureFromFile)
            {
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(picture.MediaFile!.OpenReadStream());

                streamContent.Headers.ContentType = new MediaTypeHeaderValue(picture.MediaFile.ContentType);

                content.Add(streamContent, nameof(picture.MediaFile), picture.MediaFile.FileName);

                var httpResponse = await _httpClientFactory.CreateClient("object-store-service").PostAsync("media/upload", content);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var pictureResponse = await httpResponse.Content.ReadFromJsonAsync<PictureRequest>();
                    pictures.Add(new Picture()
                    {
                        Type = Constants.PictureFromFile,
                        Url = pictureResponse!.Url!
                    });
                }
                else
                {
                    throw new Exception("Image uploading failed!");
                }
            }
        }
    }

    public async Task CreateProductAsync(ProductRequest productRequest)
    {
        try
        {
            var productCategory = await _unitOfWork.CategoryRepository.GetByIdAsync((Guid)productRequest.CategoryId!);
            if (productCategory is null)
            {
                throw new KeyNotFoundException("Category not found!");
            }
            
            ValidatePictures(productRequest.Pictures);
            
            var product = _mapper.Map<Product>(productRequest);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsActive = true;
            
            await UploadPicturesAsync(productRequest.Pictures, product.Pictures);
            
            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task UpdateProductAsync(Guid id, ProductRequest productRequest)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            
            product =  _mapper.Map(productRequest, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task DeleteProductAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }
            
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task VerifyAndUpdateProductStockAsync(List<UpdateProductStockRequest> updateProductStockRequest)
    {
        try
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync([x => 
                updateProductStockRequest.Select(p => p.Id).Contains(x.Id) && x.IsActive]);

            if (products.Count != updateProductStockRequest.Count())
            {
                throw new Exception("Some products are not available!");
            }

            await _unitOfWork.BeginTransactionAsync();

            var productDict = new Dictionary<Guid, Tuple<int, decimal>>();
            foreach (var productStockRequest in updateProductStockRequest)
            {
                productDict[productStockRequest.Id] = Tuple.Create(productStockRequest.Quantity, productStockRequest.Price);
            }

            foreach (var product in products)
            {
                if (product.SellingPrice != productDict[product.Id].Item2)
                {
                    throw new Exception("Price of some products doesn't match!");
                }
                product.Stock -=  productDict[product.Id].Item1;
            }

            await _unitOfWork.ProductRepository.UpdateRangeAsync(products);
            await _unitOfWork.SaveChangesAsync();
            
            foreach (var product in products)
            {
                if (product.Stock < 0)
                {
                    throw new Exception("Some products are out of stock!");
                }
            }
            
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw ex;
        }
    }
}