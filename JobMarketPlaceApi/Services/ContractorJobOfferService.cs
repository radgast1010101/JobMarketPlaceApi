// JobMarketPlaceApi\Services\ContractorJobOfferService.cs
using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Entities;
using JobMarketPlaceApi.Domain;

namespace JobMarketPlaceApi.Services
{
    public class ContractorJobOfferService : IContractorJobOfferService
    {
        private readonly IJobOfferRepository _offerRepo;

        public ContractorJobOfferService(IJobOfferRepository offerRepo)
        {
            _offerRepo = offerRepo ?? throw new ArgumentNullException(nameof(offerRepo));
        }

        public async Task<JobOffer> CreateOfferAsync(Guid contractorId, Guid jobId, int price, CancellationToken cancellationToken = default)
        {
            // minimal domain validation
            if (price < 0) throw new DomainException("Price must be >= 0");

            var offer = new JobOffer
            {
                Id = Guid.NewGuid(),
                ContractorId = contractorId,
                JobId = jobId,
                Price = price
            };

            return await _offerRepo.CreateAsync(offer, cancellationToken).ConfigureAwait(false);
        }

        /* Maybe Added if needed
        public async Task<JobOffer?> UpdateOfferAsync(Guid contractorId, Guid offerId, int price, CancellationToken cancellationToken = default)
        {
            if (price < 0) throw new DomainException("Price must be >= 0");

            var existing = await _offerRepo.GetByIdAsync(offerId, cancellationToken).ConfigureAwait(false);
            if (existing is null) return null;

            if (existing.ContractorId != contractorId) throw new UnauthorizedAccessException("Contractor does not own this offer");

            existing.Price = price;
            var updated = await _offerRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            return updated;
        }

        public async Task<bool> DeleteOfferAsync(Guid contractorId, Guid offerId, CancellationToken cancellationToken = default)
        {
            var existing = await _offerRepo.GetByIdAsync(offerId, cancellationToken).ConfigureAwait(false);
            if (existing is null) return false;
            if (existing.ContractorId != contractorId) throw new UnauthorizedAccessException("Contractor does not own this offer");
            return await _offerRepo.DeleteAsync(offerId, cancellationToken).ConfigureAwait(false);
        }
        */
    }
}