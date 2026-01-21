using ObjectStoreService.Application.Requests;
using ObjectStoreService.Application.Responses;

namespace ObjectStoreService.Application.IServices;

public interface IMediaService
{
    Task<MediaResponse> UploadAsync(MediaRequest mediaRequest);
}