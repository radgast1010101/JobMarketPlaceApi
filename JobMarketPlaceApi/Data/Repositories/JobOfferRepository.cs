using Microsoft.EntityFrameworkCore;
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    public class JobOfferRepository : IJobOfferRepository
    {
        private readonly JobMarketPlaceApiContext _db;

        public JobOfferRepository(JobMarketPlaceApiContext db) => _db = db;

        public Task<JobOffer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.JobOffer.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        public async Task<JobOffer> CreateAsync(JobOffer offer, CancellationToken cancellationToken = default)
        {
            //if (offer is null) throw new ArgumentNullException(nameof(offer));
            await _db.JobOffer.AddAsync(offer, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return offer;
        }

        public async Task<JobOffer?> UpdateAsync(JobOffer offer, CancellationToken cancellationToken = default)
        {
            if (offer is null) throw new ArgumentNullException(nameof(offer));

            var existing = await _db.JobOffer.FirstOrDefaultAsync(o => o.Id == offer.Id, cancellationToken).ConfigureAwait(false);
            if (existing is null) return null;

            existing.Price = offer.Price;
            existing.JobId = offer.JobId;
            // ContractorId should not change.
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var affected = await _db.JobOffer.Where(o => o.Id == id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            return affected == 1;
        }
    }
}