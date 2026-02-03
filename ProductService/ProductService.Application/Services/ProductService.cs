using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoMapper;
using ProductService.Application.IServices;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.ICache;
using ProductService.Domain.ILogging;
using ProductService.Domain.IRepositories;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Constants;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICache _cache;
    private readonly IAppLogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IHttpClientFactory httpClientFactory, ICache cache, 
        IAppLogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProductResponse>> GetAllProductsAsync(GetAllProductsFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all products from database");
            List<Expression<Func<Product, bool>>> filters = [];
            filters.Add(x => x.IsActive == filter.IsActive);
            if (filter.Name is not null) filters.Add(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

            var products = await _unitOfWork.ProductRepository.GetAllAsync(filters,
                [x => x.Category!, x => x.Pictures.Where(p => p.IsActive)],
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
            _logger.LogError(ex, "Failed to get all products from database");
            throw;
        }
    }
    
    public async Task<PaginatedResponse<ProductResponse>> GetAllProductsFromRedisAsync(GetAllProductsFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting all products from redis");
            string query =  $"@IsActive:{{{filter.IsActive}}} ";
            if (filter.CategoryName is not null)
            {
                query += $"@CategoryName:{filter.CategoryName} ";
            }
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query += $"(@Name:{filter.Name + "*"} | @Name:{filter.Name})";
            }

            var products = await _cache.GetAllJsonAsync<ProductResponse>(Constants.ProductCacheIndex, 
                query, filter.PageNumber, filter.ItemsPerPage, "CreatedAt", "DESC");
            var productCount = await _cache.GetAllCountAsync(Constants.ProductCacheIndex, query);

            var paginatedResponse = new PaginatedResponse<ProductResponse>(
                products,
                productCount,
                filter.ItemsPerPage,
                filter.PageNumber);

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all products");
            throw;
        }
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Getting product with id {id} from redis");
            string? product = await _cache.GetJsonAsync(Constants.ProductCacheKeyPrefix + id);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }

            return JsonSerializer.Deserialize<ProductResponse>(product)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get product with id {id} from redis");
            throw;
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
            throw new ArgumentException("You are not allowed to upload more than five pictures!");
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

                var httpResponse = await _httpClientFactory.CreateClient("object-store-service")
                    .PostAsync("media/upload", content);
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

    private Product MapToProduct(ProductRequest productUpdateRequest)
    {
        return new Product()
        {
            Name = productUpdateRequest.Name!,
            Description = productUpdateRequest.Description!,
            BuyPrice = (decimal)productUpdateRequest.BuyPrice!,
            SellingPrice = (decimal)productUpdateRequest.SellingPrice!,
            Stock = (int)productUpdateRequest.Stock!,
            CategoryId = (Guid)productUpdateRequest.CategoryId!,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void MapToProduct(ProductUpdateRequest productUpdateRequest, Product product)
    {
        product.Name = productUpdateRequest.Name!;
        product.Description = productUpdateRequest.Description!;
        product.BuyPrice = (decimal)productUpdateRequest.BuyPrice!;
        product.SellingPrice = (decimal)productUpdateRequest.SellingPrice!;
        product.Stock = (int)productUpdateRequest.Stock!;
        product.CategoryId = (Guid)productUpdateRequest.CategoryId!;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public async Task CreateProductAsync(ProductRequest productRequest)
    {
        try
        {
            _logger.LogInformation("Creating new product");
            var productCategory = await _unitOfWork.CategoryRepository.GetByIdAsync((Guid)productRequest.CategoryId!);

            if (productCategory is null)
            {
                throw new KeyNotFoundException("Category not found!");
            }

            ValidatePictures(productRequest.Pictures);

            var product = MapToProduct(productRequest);
            await UploadPicturesAsync(productRequest.Pictures, product.Pictures);
            
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            await _cache.SetJsonAsync(Constants.ProductCacheKeyPrefix + product.Id, _mapper.Map<ProductResponse>(product));

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to create product");
            throw;
        }
    }

    public async Task UpdateProductAsync(Guid id, ProductUpdateRequest productRequest)
    {
        try
        {
            _logger.LogInformation("Updating product");
            var product = await _unitOfWork.ProductRepository.GetAsync([x => x.Id == id],
                [x => x.Pictures.Where(p => p.IsActive)]);
            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }

            if (productRequest.Pictures.Count + product.Pictures.Count - productRequest.DeletePictureIds.Count > 5)
            {
                throw new ArgumentException("You are not allowed to upload more than five pictures!");
            }

            ValidatePictures(productRequest.Pictures);

            foreach (var picture in product.Pictures)
            {
                if (productRequest.DeletePictureIds.Contains(picture.Id))
                {
                    picture.IsActive = false;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            MapToProduct(productRequest, product);
            await UploadPicturesAsync(productRequest.Pictures, product.Pictures);

            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            await _cache.SetJsonAsync(Constants.ProductCacheKeyPrefix + product.Id, _mapper.Map<ProductResponse>(product));

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task DeleteProductAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting product");
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);

            if (product is null)
            {
                throw new Exception($"Product with id {id} not found");
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            await _cache.DeleteJsonAsync(Constants.ProductCacheKeyPrefix + product.Id);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task VerifyAndUpdateProductStockAsync(List<UpdateProductStockRequest> updateProductStockRequest)
    {
        try
        {
            _logger.LogInformation("Verifying to update product stock");
            var products = await _unitOfWork.ProductRepository.GetAllAsync([
                x =>
                    updateProductStockRequest.Select(p => p.Id).Contains(x.Id) && x.IsActive
            ]);

            if (products.Count != updateProductStockRequest.Count())
            {
                throw new ArgumentException("Some products are not available!");
            }

            var productDict = new Dictionary<Guid, Tuple<int, decimal>>();
            foreach (var productStockRequest in updateProductStockRequest)
            {
                productDict[productStockRequest.Id] =
                    Tuple.Create(productStockRequest.Quantity, productStockRequest.Price);
            }

            foreach (var product in products)
            {
                if (product.SellingPrice != productDict[product.Id].Item2)
                {
                    throw new ArgumentException("Price of some products doesn't match!");
                }

                product.Stock -= productDict[product.Id].Item1;
            }
            
            await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.ProductRepository.UpdateRangeAsync(products);
            await _unitOfWork.SaveChangesAsync();

            foreach (var product in products)
            {
                if (product.Stock < 0)
                {
                    throw new ArgumentException("Some products are out of stock!");
                }
            }

            foreach (var product in products)
            {
                await _cache.SetJsonAsync(Constants.ProductCacheKeyPrefix + product.Id,"Stock", product.Stock);
            }

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to update product stock");
            throw;
        }
    }
}