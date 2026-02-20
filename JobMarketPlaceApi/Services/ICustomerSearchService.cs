// JobMarketPlaceApi\Services\ICustomerSearchService.cs
using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Services
{
    public interface ICustomerSearchService
    {
        // Validate inputs, clamp limits, forward to repository and return repository result.
        Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}