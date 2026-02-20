// JobMarketPlaceApi\Services\ICustomerJobService.cs
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Services
{
    public interface ICustomerJobService
    {
        // Orchestrates: load customer, create job (domain factory), persist via repository.
        Task<Job> CreateJobForCustomerAsync(Guid customerId, string description, DateTime startDate, DateTime? dueDate, int? budget, CancellationToken cancellationToken = default);
    }
}