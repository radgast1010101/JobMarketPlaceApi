using Microsoft.EntityFrameworkCore;
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly JobMarketPlaceApiContext _db;
        private const int MaxPageSize = 100;
        private const int MinPrefixLength = 3;

        public CustomerRepository(JobMarketPlaceApiContext db) => _db = db;

        // Simple pagination is used,  avoids complexity of cursor management.
        // Offset-based cursor isn't needed; use pageNumber/pageSize in endpoints.
        public async Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // trim and validate prefix
            var trimmed = (prefix ?? string.Empty).Trim(); 
            if (trimmed.Length < MinPrefixLength)
            {
                return new SearchResult<Customer>(new List<Customer>(), false, null, null);
            }

            var size = Math.Clamp(pageSize, 1, MaxPageSize);
            var skip = Math.Max(pageNumber - 1, 0) * size;
            var fetch = size + 1;

            var query = _db.Customer
                .AsNoTracking()
                .Where(c => EF.Functions.Like(c.LastName, trimmed + "%"))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.Id);

            var rows = await query
                .Skip(skip)
                .Take(fetch)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var hasMore = rows.Count == fetch;
            if (hasMore) rows.RemoveAt(rows.Count - 1);

            return new SearchResult<Customer>(rows, hasMore, null, null);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.Customer.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}