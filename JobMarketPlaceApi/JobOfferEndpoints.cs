using Microsoft.EntityFrameworkCore;
using JobMarketPlaceApi.Data;
using JobMarketPlaceApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
namespace JobMarketPlaceApi;

public static class JobOfferEndpoints
{
    public static void MapJobOfferEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/JobOffer").WithTags(nameof(JobOffer));

        group.MapGet("/", async (JobMarketPlaceApiContext db) =>
        {
            return await db.JobOffer.ToListAsync();
        })
        .WithName("GetAllJobOffers")
        .WithOpenApi()
        .RequireAuthorization();


        group.MapGet("/{id}", async Task<Results<Ok<JobOffer>, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
        {
            return await db.JobOffer.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is JobOffer model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetJobOfferById")
        .WithOpenApi()
        .RequireAuthorization();


        #region AdminOnlyAPI

        /* Update job offer
        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (Guid id, JobOffer jobOffer, JobMarketPlaceApiContext db) =>
        {
            var affected = await db.JobOffer
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, jobOffer.Id)
                    .SetProperty(m => m.JobId, jobOffer.JobId)
                    .SetProperty(m => m.Price, jobOffer.Price)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateJobOffer")
        .WithOpenApi()
        .RequireAuthorization();
        */

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
        {
            var affected = await db.JobOffer
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteJobOffer")
        .WithOpenApi()
        .RequireAuthorization();
            
        #endregion

    }
}
