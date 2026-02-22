
using Microsoft.EntityFrameworkCore;
using JobMarketPlaceApi.Data;
using JobMarketPlaceApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JobMarketPlaceApi
{
    public static class ContractorEndpoints
    {
        // DTOs returned by the API (items contain Name+Rating only as requested)
        public record ContractorDto(string Name, int Rating);
        public record ContractorSearchResponse(
            List<ContractorDto> Items,
            bool HasMore,
            string? NextName,
            Guid? NextId
        );

        public static void MapContractorEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Contractor").WithTags(nameof(Contractor));

            // Exact lookup by id, Non Repository Pattern
            group.MapGet("/{id}", async Task<Results<Ok<ContractorDto>, NotFound>> (
                    Guid id,
                    /*  Cache: IMemoryCache cache*/

                    /*  JobMarketPlaceApiContext db, improved by using repository/service interfaces*/
                    JobMarketPlaceApiContext db) =>
            {
                var c = await db.Contractor
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                #region CacheConsiderations
                /* Cache:
                 * 
                 *                 var cacheKey = $"contractor:id:{id}";

                if (!cache.TryGetValue(cacheKey, out ContractorDto? cachedDto))
                {
                    var c = await db.Contractor
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (c is null)
                    {
                        return TypedResults.NotFound();
                    }

                    cachedDto = new ContractorDto(c.Name, c.Rating);

                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    }.SetSize(1);

                    cache.Set(cacheKey, cachedDto, cacheEntryOptions);
                }
                 * */
                #endregion

                return c is not null
                    ? TypedResults.Ok(new ContractorDto(c.Name, c.Rating))
                    : TypedResults.NotFound();
            })
                .WithName("GetContractorById")
                .WithOpenApi()
                .RequireAuthorization();


            // Search contractors: Prefix search with keyset pagination using provider-safe raw SQL for the keyset predicate.
            // Route: GET /api/Contractor/search/{prefix}?limit=20&afterName=...&afterId=...
            group.MapGet("/search/{prefix}", async Task<Results<Ok<ContractorSearchResponse>, BadRequest>> (
                    string prefix,
                    int? limit,
                    string? afterName,
                    Guid? afterId,
                    /*  Cache: IMemoryCache cache*/

                    /*  context db, improved by using repository/service interfaces, see customer */
                    JobMarketPlaceApiContext db) =>
            {
                // trim and validate prefix
                var trimmed = (prefix ?? string.Empty).Trim();
                if (trimmed.Length < 3) // minimum prefix length enforced
                {
                    return TypedResults.BadRequest();
                }

                var pageSize = Math.Clamp(limit ?? 20, 1, 100); // validate, clamp or limit = 100
                var fetch = pageSize + 1; // fetch one extra to detect HasMore

                // Build cache key including cursor so each page has a distinct key
                var cursorPart = (afterName is null || !afterId.HasValue) ? "start" : $"after:{afterName}:{afterId}";
                var cacheKey = $"contractor:search:{trimmed}:l{pageSize}:{cursorPart}";

                #region CacheConsiderations
                /* Cache:
                 * 
                if (!cache.TryGetValue(cacheKey, out ContractorSearchResponse? cachedResponse))
                {
                    List<Contractor> rows;
                    ...
                    if (hasMore && rows.Count > 0) {...}


                    cachedResponse = new ContractorSearchResponse(items, hasMore, nextName, nextId);
 
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    }.SetSize(1);

                    cache.Set(cacheKey, cachedResponse, cacheEntryOptions);
                }
                 * 
                 */
                #endregion

                // Use parameterized raw SQL for deterministic translation of keyset predicate.
                // else: When no cursor is present use a simpler SQL with LIMIT and ORDER BY.
                List<Contractor> rows;
                if (!string.IsNullOrEmpty(afterName) && afterId.HasValue)
                {
                    // safe parameterization via FromSqlInterpolated
                    rows = await db.Contractor
                        .FromSqlInterpolated($@"
                                SELECT Id, Name, Rating
                                FROM Contractor
                                WHERE Name LIKE {trimmed + "%"}
                                  AND ( (Name > {afterName}) OR (Name = {afterName} AND Id > {afterId}) )
                                ORDER BY Name, Id
                                LIMIT {fetch}")
                        .AsNoTracking()
                        .ToListAsync();
                }
                else
                {
                    /* simpler SQL that includes LIMIT and ORDER BY */
                    rows = await db.Contractor
                         .FromSqlInterpolated($@"
                                 SELECT Id, Name, Rating
                                 FROM Contractor
                                 WHERE Name LIKE {trimmed + "%"}
                                 ORDER BY Name, Id
                                 LIMIT {fetch}")
                         .AsNoTracking()
                         .ToListAsync();
                 }


                var hasMore = rows.Count == fetch;
                if (hasMore)
                {
                    rows.RemoveAt(rows.Count - 1); // drop extra sentinel row
                }

                var items = rows.Select(r => new ContractorDto(r.Name, r.Rating)).ToList();

                string? nextName = null;
                Guid? nextId = null;
                if (hasMore && rows.Count > 0)
                {
                    var last = rows[^1];
                    nextName = last.Name;
                    nextId = last.Id;
                }

                var response = new ContractorSearchResponse(items, hasMore, nextName, nextId);
                return TypedResults.Ok(response);
            })
                .WithName("SearchContractors")
                .WithOpenApi()
                .RequireAuthorization();


            // Create job offer
            group.MapPost("/{contractorId:guid}/joboffers", async Task<Results<Created<JobOffer>, BadRequest<string>>> (
                    Guid contractorId,
                    JobOffer input,
                    JobMarketPlaceApi.Services.IContractorJobOfferService service) =>
            {
                // Basic validation
                if (input == null) return TypedResults.BadRequest("payload required");
                if (input.JobId == Guid.Empty) return TypedResults.BadRequest("JobId required");
                if (input.Price < 0) return TypedResults.BadRequest("Price must be >= 0");

                var created = await service.CreateOfferAsync(contractorId, input.JobId, input.Price);
                return TypedResults.Created($"/api/JobOffer/{created.Id}", created);
            })
            .WithName("CreateJobOfferForContractor")
            .WithOpenApi()
            .RequireAuthorization();

            #region AdminOnly
            group.MapPost("/", async (Contractor contractor, JobMarketPlaceApiContext db) =>
            {
                db.Contractor.Add(contractor);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Contractor/{contractor.Id}", contractor);
            })
            .WithName("CreateContractor")
            .WithOpenApi()
            .RequireAuthorization();
            
            group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
            {
                var affected = await db.Contractor
                    .Where(model => model.Id == id)
                    .ExecuteDeleteAsync();
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("DeleteContractor")
            .WithOpenApi()
            .RequireAuthorization();

            #endregion
        }
    }
}
