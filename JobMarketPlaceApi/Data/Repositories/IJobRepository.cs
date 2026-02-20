// JobMarketPlaceApi\Data\Repositories\IJobRepository.cs
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    public interface IJobRepository
    {
        // Persist job (SaveChanges) and return persisted entity.
        Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    }
}