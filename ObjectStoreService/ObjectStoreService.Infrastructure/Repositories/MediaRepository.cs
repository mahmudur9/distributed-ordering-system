using ObjectStoreService.Domain.IRepositories;
using ObjectStoreService.Domain.Models;
using ObjectStoreService.Infrastructure.Data;

namespace ObjectStoreService.Infrastructure.Repositories;

public class MediaRepository : Repository<Media>, IMediaRepository
{
    public MediaRepository(DBContext context) : base(context)
    {
    }
}