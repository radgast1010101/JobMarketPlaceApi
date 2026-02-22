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
            // Price validation
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
    }
}