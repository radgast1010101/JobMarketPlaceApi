using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    // Generic search result used across repository/service boundary.
    public record SearchResult<T>(List<T> Items, bool HasMore, string? NextKey, Guid? NextId);

    public interface ICustomerRepository
    {
        // Offset pagination: pageNumber is 1-based, pageSize capped by implementation.
        Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        // Exact lookup
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}