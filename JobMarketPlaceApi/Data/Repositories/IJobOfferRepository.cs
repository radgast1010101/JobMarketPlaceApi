using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    public interface IJobOfferRepository
    {
      Task<JobOffer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
      Task<JobOffer> CreateAsync(JobOffer offer, CancellationToken cancellationToken = default);
      Task<JobOffer?> UpdateAsync(JobOffer offer, CancellationToken cancellationToken = default);
      Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}