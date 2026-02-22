using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Services
{
    public class CustomerSearchService : ICustomerSearchService
    {
        private readonly ICustomerRepository _repository;
        private const int MinPrefixLength = 3;
        private const int MaxPageSize = 100;

        public CustomerSearchService(ICustomerRepository repository) => _repository = repository;

        public async Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var trimmed = (prefix ?? string.Empty).Trim();

            if (trimmed.Length < MinPrefixLength)
                throw new ArgumentException($"prefix must be at least {MinPrefixLength} characters long", nameof(prefix));

            var effectivePage = Math.Max(pageNumber, 1);
            var effectiveSize = Math.Clamp(pageSize, 1, MaxPageSize);

            // Service responsibility: validate, clamp and forward; repository handles actual data access.
            return await _repository.SearchByLastNamePrefixAsync(trimmed, effectivePage, effectiveSize, cancellationToken);
        }
    }
}