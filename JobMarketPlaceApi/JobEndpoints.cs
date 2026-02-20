using Microsoft.EntityFrameworkCore;
using JobMarketPlaceApi.Data;
using JobMarketPlaceApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
namespace JobMarketPlaceApi;

public static class JobEndpoints
{
    public static void MapJobEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Job").WithTags(nameof(Job));

        group.MapGet("/", async (JobMarketPlaceApiContext db) =>
        {
            return await db.Job.ToListAsync();
        })
        .WithName("GetAllJobs")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Job>, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
        {
            return await db.Job.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Job model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetJobById")
        .WithOpenApi();

        #region AdminOnlyAPI
        // maybe Admin only
        //group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (Guid id, Job job, JobMarketPlaceApiContext db) =>
        //{
        //    var affected = await db.Job
        //        .Where(model => model.Id == id)
        //        .ExecuteUpdateAsync(setters => setters
        //            .SetProperty(m => m.Id, job.Id)
        //            .SetProperty(m => m.Description, job.Description)
        //            .SetProperty(m => m.AcceptedBy, job.AcceptedBy)
        //            .SetProperty(m => m.Budget, job.Budget)
        //            .SetProperty(m => m.StartDate, job.StartDate)
        //            .SetProperty(m => m.DueDate, job.DueDate)
        //            );
        //    return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        //})
        //.WithName("UpdateJob")
        //.WithOpenApi();

        // maybe Admin only
        //group.MapPost("/", async (Job job, JobMarketPlaceApiContext db) =>
        //{
        //    db.Job.Add(job);
        //    await db.SaveChangesAsync();
        //    return TypedResults.Created($"/api/Job/{job.Id}",job);
        //})
        //.WithName("CreateJob")
        //.WithOpenApi();

        // maybe Admin only
        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (Guid id, JobMarketPlaceApiContext db) =>
        {
            var affected = await db.Job
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteJob")
        .WithOpenApi()
        .RequireAuthorization();
        #endregion
    }
}
