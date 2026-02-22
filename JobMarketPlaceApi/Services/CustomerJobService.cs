using JobMarketPlaceApi.Data.Repositories;
using JobMarketPlaceApi.Entities;

namespace JobMarketPlaceApi.Services
{
    public class CustomerJobService : ICustomerJobService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IJobRepository _jobRepository;

        public CustomerJobService(ICustomerRepository customerRepository, IJobRepository jobRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        }

        public async Task<Job> CreateJobForCustomerAsync(Guid customerId, string description, DateTime startDate, 
            DateTime? dueDate, int? budget, CancellationToken cancellationToken = default)
        {
            // Load customer (service keeps orchestration; repository handles data access)
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken).ConfigureAwait(false);
            if (customer is null)
                throw new InvalidOperationException("Customer not found");

            // Use domain factory to enforce invariants
            var job = customer.CreateJob(description, startDate, dueDate, budget);

            // Persist
            var created = await _jobRepository.CreateAsync(job, cancellationToken).ConfigureAwait(false);
            return created;
        }
    }
}