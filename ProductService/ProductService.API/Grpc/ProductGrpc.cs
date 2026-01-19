using Grpc.Core;
using ProductService.Application.IServices;
using ProductService.Application.Requests;

namespace ProductService.API.Grpc;

public class ProductGrpc : ProductGrpcService.ProductGrpcServiceBase
{
    private readonly IProductService _productService;

    public ProductGrpc(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<GrpcResponse> VerifyAndUpdateProductStock(UpdateProductGrpcStockRequest request, ServerCallContext context)
    {
        var resposne = new GrpcResponse();
        try
        {
            var stocks = new List<UpdateProductStockRequest>();
            foreach (var product in request.Products)
            {
                stocks.Add(new UpdateProductStockRequest()
                {
                    Id = Guid.Parse(product.Id),
                    Quantity = product.Quantity,
                    Price = decimal.Parse(product.Price)
                });
            }
            
            await _productService.VerifyAndUpdateProductStockAsync(stocks);

            resposne.Success = true;
            return resposne;
        }
        catch (Exception ex)
        {
            resposne.Success = false;
            resposne.Error = ex.Message;
            return resposne;
        }
    }
}