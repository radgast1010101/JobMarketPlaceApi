using JobMarketPlaceApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMarketPlaceApi.Data
{
    public class JobMarketPlaceApiContext : DbContext
    {
        public JobMarketPlaceApiContext(DbContextOptions<JobMarketPlaceApiContext> options)
            : base(options)
        {
        }

        public DbSet<JobMarketPlaceApi.Entities.Job> Job { get; set; } = default!;
        public DbSet<JobMarketPlaceApi.Entities.JobOffer> JobOffer { get; set; } = default!;
        public DbSet<JobMarketPlaceApi.Entities.Customer> Customer { get; set; } = default!;
        public DbSet<JobMarketPlaceApi.Entities.Contractor> Contractor { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.LastName)
                      .IsRequired()
                      .UseCollation("NOCASE"); // case-insensitive prefix searches in SQLite

                entity.Property(e => e.FirstName)
                      .IsRequired();

                // Composite index (LastName, Id) supports ordered keyset pagination
                //entity.HasIndex(e => new { e.LastName, e.Id })
                      //.HasDatabaseName("IX_Customer_LastName_Id");
            });

            modelBuilder.Entity<Contractor>(entity =>
            {
                entity.Property(e => e.Name)
                      .IsRequired()
                      .UseCollation("NOCASE"); // case-insensitive prefix searches in SQLite

                // Composite index (Name, Id) supports ordered keyset pagination
                entity.HasIndex(e => new { e.Name, e.Id })
                      .HasDatabaseName("IX_Contractor_Name_Id");
            });
        }
    }
}

/*
 * Repository Pattern: 
 * small Repository + Specification-like implementations for Customer and Contractor search. 
 * They encapsulate EF Core queries, enforce the prefix rules and pagination logic, 
 * and are easy to call from your endpoints. 
 * Endpoints become thin: validation + mapping + HTTP semantics; data access is reusable and swapable
 * I register them in DI so endpoints can accept the interfaces instead of JobMarketPlaceApiContext.
 * 
 * add JobMarketPlaceApi\Data\Repositories\ICustomerRepository.cs
 * add JobMarketPlaceApi\Data\Repositories\IContractorRepository.cs
 * add JobMarketPlaceApi\Data\Repositories\CustomerRepository.cs
 * add JobMarketPlaceApi\Data\Repositories\ContractorRepository.cs
 * 
 * at JobMarketPlaceApi\Program.cs, after builder.Services.AddDbContext<...>(...)
 * builder.Services.AddScoped<JobMarketPlaceApi.Data.Repositories.ICustomerRepository, JobMarketPlaceApi.Data.Repositories.CustomerRepository>();
 * builder.Services.AddScoped<JobMarketPlaceApi.Data.Repositories.IContract
 * 
 */

/*JobMarketPlaceApi\Data\Repositories\ICustomerRepository.cs
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Data.Repositories
{
    public record SearchResult<T>(List<T> Items, bool HasMore, string? NextKey, Guid? NextId);

    public interface ICustomerRepository
    {
        // Offset pagination: pageNumber is 1-based, pageSize capped by implementation.
        Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        // Exact lookup
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

JobMarketPlaceApi\Data\Repositories\CustomerRepository.cs
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

        public async Task<SearchResult<Customer>> SearchByLastNamePrefixAsync(string prefix, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
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

            // Offset-based cursor isn't needed; use pageNumber/pageSize in endpoints.
            return new SearchResult<Customer>(rows, hasMore, null, null);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _db.Customer.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
 
 */



