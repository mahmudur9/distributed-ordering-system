using System.Globalization;
using OrderService.Application.Abstractions.Gateways;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using ProductService.API;

namespace OrderService.Infrastructure.GrpcClients;

public class ProductGrpcClient : IProductGateway
{
    private readonly ProductGrpcService.ProductGrpcServiceClient _productGrpcServiceClient;

    public ProductGrpcClient(ProductGrpcService.ProductGrpcServiceClient productGrpcServiceClient)
    {
        _productGrpcServiceClient = productGrpcServiceClient;
    }

    public async Task<GatewayResponse> VerifyAndUpdateProductStockAsync(List<ProductStockGatewayRequest> productStockRequests)
    {
        var productStock = new UpdateProductGrpcStockRequest();
        foreach (var product in productStockRequests)
        {
            productStock.Products.Add(new ProductStockGrpcRequest()
            {
                Id = product.Id.ToString(),
                Quantity = product.Quantity,
                Price = product.Price.ToString(CultureInfo.InvariantCulture)
            });
        }
        var productStockResponse = await _productGrpcServiceClient.VerifyAndUpdateProductStockAsync(productStock);
        return new GatewayResponse()
        {
            Success =  productStockResponse.Success,
            Error = productStockResponse.Error
        };
    }
}