using System.Net.Http.Json;
using ProductService.Application.Abstractions.Gateways;

namespace ProductService.Infrastructure.HttpClients;

public class ObjectStoreClient : IObjectStoreGateway
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ObjectStoreClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<T> UploadFileAsync<T>(MultipartFormDataContent content)
    {
        var httpResponse = await _httpClientFactory.CreateClient("object-store-service")
            .PostAsync("media/upload", content);
        if (httpResponse.IsSuccessStatusCode)
        {
            var response = await httpResponse.Content.ReadFromJsonAsync<T>();
            return response!;
        }
        else
        {
            throw new Exception("File uploading failed!");
        }
    }
}