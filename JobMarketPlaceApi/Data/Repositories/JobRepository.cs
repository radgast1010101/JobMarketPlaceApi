// JobMarketPlaceApi\Data\Repositories\JobRepository.cs
using JobMarketPlaceApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMarketPlaceApi.Data.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly JobMarketPlaceApiContext _db;

        public JobRepository(JobMarketPlaceApiContext db) => _db = db;

        public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
        {
            if (job is null) throw new ArgumentNullException(nameof(job));
            await _db.Job.AddAsync(job, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return job;
        }
    }
}