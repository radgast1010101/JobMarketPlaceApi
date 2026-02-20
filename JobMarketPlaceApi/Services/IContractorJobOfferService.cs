// JobMarketPlaceApi\Services\IContractorJobOfferService.cs
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Services
{
    public interface IContractorJobOfferService
    {
        Task<JobOffer> CreateOfferAsync(Guid contractorId, Guid jobId, int price, CancellationToken cancellationToken = default);
        
        // uncomment/add, if needed
        //Task<JobOffer?> UpdateOfferAsync(Guid contractorId, Guid offerId, int price, CancellationToken cancellationToken = default);
        //Task<bool> DeleteOfferAsync(Guid contractorId, Guid offerId, CancellationToken cancellationToken = default);
    }
}