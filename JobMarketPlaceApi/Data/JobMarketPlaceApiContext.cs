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

                // Composite index (Name, Id) supports ordered keyset pagination (see Contractor search endpoint)
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
 * Register them in DI so endpoints can accept the interfaces instead of JobMarketPlaceApiContext.
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