
//using JobMarketPlaceApi.Data;

using JobMarketPlaceApi.Entities;
using JobMarketPlaceApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

//using Microsoft.EntityFrameworkCore;
// cache: using Microsoft.Extensions.Caching.Memory;

namespace JobMarketPlaceApi
{
    public static class CustomerEndpoints
    {
        // DTOs returned by the API, also immutable, concurrency safe
        public record CustomerDto(Guid Id, string FirstName, string LastName);
        public record CustomerSearchResponse(
            List<CustomerDto> Items,
            bool HasMore,
            int Page,
            int PageSize
        );



        public record JobDto(Guid Id, string Description, string AcceptedBy, int Budget,
            DateTime StartDate, DateTime DueDate);

        public record CreateJobResponse(JobDto CreatedJob);

        public record CreateJobRequest(string Description, DateTime StartDate, DateTime? DueDate = null, int? Budget = null);


        #region NonRepositoryPattern
        /*

                public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
                {
                    var group = routes.MapGroup("/api/Customer").WithTags(nameof(Customer));

                    // Offset pagination: /api/Customer/search/{prefix}?page=1&size=20
                    group.MapGet("/search/{prefix}", async Task<Results<Ok<CustomerSearchResponse>, BadRequest>> (
                            string prefix,
                            int? page,
                            int? size,
                            // Cache: IMemoryCache cache,

                            // JobMarketPlaceApiContext db, improved by always using repository/service interfaces
                            JobMarketPlaceApiContext db) =>
                    {
                        // Validation: minimum prefix length (3)
                        var trimmed = (prefix ?? string.Empty).Trim();
                        if (trimmed.Length < 3)
                        {
                            return TypedResults.BadRequest();
                        }

                        // Pagination parameters and validation
                        var pageNumber = Math.Max(page ?? 1, 1);
                        var pageSize = Math.Clamp(size ?? 20, 1, 100);

                        // Cache:
                                    // Build a cache key that includes prefix + paging so different pages are cached separately
                                    //var cacheKey = $"customer:search:{trimmed}:p{pageNumber}:s{pageSize}";
                                    // Try to get cached response; if missing, populate and cache it.
                                    if (!cache.TryGetValue(cacheKey, out CustomerSearchResponse? cached))
                                    {
                                        var skip = (pageNumber - 1) * pageSize;
                                        ...
                                        ...
                                        if (hasMore) {...}

                                        cached = new CustomerSearchResponse(results, hasMore, pageNumber, pageSize);

                                         // Cache the response with size=1 and sliding expiration
                                        var cacheEntryOptions = new MemoryCacheEntryOptions
                                        {
                                            SlidingExpiration = TimeSpan.FromMinutes(5)
                                        }.SetSize(1);

                                        cache.Set(cacheKey, cached, cacheEntryOptions);
                            //         }


                        var skip = (pageNumber - 1) * pageSize;
                        var fetch = pageSize + 1; // fetch one extra to detect hasMore

                        // Base query: case-insensitive prefix using LIKE (NOCASE collation applied at model)
                        var query = db.Customer
                                      .AsNoTracking()
                                      .Where(c => EF.Functions.Like(c.LastName, trimmed + "%"));

                        var results = await query
                            .OrderBy(c => c.LastName)
                            .ThenBy(c => c.Id)
                            .Skip(skip)
                            .Take(fetch)
                            .Select(c => new CustomerDto(c.Id, c.FirstName, c.LastName))
                            .ToListAsync();

                        var hasMore = results.Count == fetch;
                        if (hasMore)
                        {
                            // remove the extra item from results
                            results.RemoveAt(results.Count - 1);
                        }

                        var response = new CustomerSearchResponse(results, hasMore, pageNumber, pageSize);

                        return TypedResults.Ok(response);
                    })
                        .WithName("SearchCustomers")
                        .WithOpenApi()
                        .RequireAuthorization(); // placeholder: requires configured auth middleware
                }

            */
        #endregion

        public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Customer").WithTags(nameof(Customer));

            // Offset pagination: /api/Customer/search/{prefix}?page=1&size=20
            group.MapGet("/search/{prefix}", async Task<Results<Ok<CustomerSearchResponse>, BadRequest>> (
                    string prefix,
                    int? page,
                    int? size,
                    ICustomerSearchService customerSearchService) =>
            {
                // Use service to validate and perform the search. Service throws ArgumentException for invalid input.
                try
                {
                    var pageNumber = Math.Max(page ?? 1, 1);
                    var pageSize = Math.Clamp(size ?? 20, 1, 100); // limited or clamped to <= 100

                    var repoResult = await customerSearchService.SearchByLastNamePrefixAsync(prefix, pageNumber, pageSize);

                    var items = repoResult.Items
                        .Select(c => new CustomerDto(c.Id, c.FirstName, c.LastName))
                        .ToList();

                    var response = new CustomerSearchResponse(items, repoResult.HasMore, pageNumber, pageSize);
                    return TypedResults.Ok(response);
                }
                catch (ArgumentException)
                {
                    return TypedResults.BadRequest();
                }
            })
                .WithName("SearchCustomers")
                .WithOpenApi()
                .RequireAuthorization(); // placeholder: requires configured auth middleware

            // create jobs
            group.MapPost("/{customerId:guid}/jobs", 
                async Task<Results<Created<object>, BadRequest<string>, NotFound, IResult>> (
                    Guid customerId,
                    CreateJobRequest request,
                    ICustomerJobService customerJobService,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken) =>
            {
                // Input validation (defensive, keep endpoint thin)
                if (string.IsNullOrWhiteSpace(request.Description))
                    return TypedResults.BadRequest("Description is required");

                if (request.StartDate == default)
                    return TypedResults.BadRequest("StartDate is required");


                // simple auth: subject must match customer id
                var subject = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(subject) || !Guid.TryParse(subject, out var subjectGuid) || subjectGuid != customerId)
                    return TypedResults.Forbid();


                try
                {
                    var created = await customerJobService.CreateJobForCustomerAsync(customerId,
                        request.Description, request.StartDate, request.DueDate, request.Budget, 
                        cancellationToken).ConfigureAwait(false);


                    return TypedResults.Created($"/api/Job/{created.Id}", new
                    {
                        created.Id,
                        created.CustomerId,
                        created.Description,
                        created.AcceptedBy,
                        created.Budget,
                        created.StartDate,
                        created.DueDate
                    });
                }
                catch (InvalidOperationException) // customer not found
                {
                    return TypedResults.NotFound();
                }
                catch (Exception ex) when (ex is JobMarketPlaceApi.Domain.DomainException || ex is ArgumentException)
                {
                    // domain validation failure
                    return TypedResults.BadRequest(ex.Message);
                }
            })
                .WithName("CreateJobForCustomer")
                .WithOpenApi()
                .RequireAuthorization();

        }
    }
}