using JobMarketPlaceApi.Data;
using JobMarketPlaceApi.Entities;
using JobMarketPlaceApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
//using System.Security.Claims; TEMP: use dev auth atm

namespace JobMarketPlaceApi
{
    public static class CustomerEndpoints
    {
        // DTOs returned by the API, also immutable, concurrency-safe
        // Maybe separated to a module/folder later
        public record CustomerDto(Guid Id, string FirstName, string LastName);
        public record CustomerSearchResponse(
            List<CustomerDto> Items,
            bool HasMore,
            int Page,
            int PageSize
        );
        public record CreateJobRequest(string Description, DateTime StartDate, DateTime? DueDate = null, int? Budget = null);

        public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Customer").WithTags(nameof(Customer));

            // Search customers: /api/Customer/search/{prefix}?page=1&size=20
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

            // Create jobs
            group.MapPost("/{customerId:guid}/jobs", 
                async Task<Results<Created<object>, BadRequest<string>, NotFound, IResult>> (
                    Guid customerId,
                    CreateJobRequest request,
                    ICustomerJobService customerJobService,
                    //ClaimsPrincipal user, TEMP: use dev auth atm
                    CancellationToken cancellationToken) =>
            {
                // Input validation (defensive, keep endpoint thin)
                if (string.IsNullOrWhiteSpace(request.Description))
                    return TypedResults.BadRequest("Description is required");

                if (request.StartDate == default)
                    return TypedResults.BadRequest("StartDate is required");


                // TEMP: use dev auth atm, not below
                // simple auth: subject must match customer id
                //var subject = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                //if (string.IsNullOrEmpty(subject) || !Guid.TryParse(subject, out var subjectGuid) || subjectGuid != customerId)
                //    return TypedResults.Forbid();


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

            #region AdminOnly
            // Simple, Non Repository Pattern endpoints below, for demo purposes only
            group.MapPost("/", async (Customer customer, JobMarketPlaceApiContext db) =>
            {
                db.Customer.Add(customer);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Customer/{customer.Id}", customer);
            })
            .WithName("CreateCustomer")
            .WithOpenApi()
            .RequireAuthorization();

            group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
            {
                var affected = await db.Customer
                    .Where(model => model.Id == id)
                    .ExecuteDeleteAsync();
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("DeleteCustomer")
            .WithOpenApi()
            .RequireAuthorization();

            #endregion
        }
    }
}