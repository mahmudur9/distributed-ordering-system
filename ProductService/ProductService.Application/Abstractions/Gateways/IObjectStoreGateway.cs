namespace ProductService.Application.Abstractions.Gateways;

public interface IObjectStoreGateway
{
    Task<T> UploadFileAsync<T>(MultipartFormDataContent content);
}