using JobMarketPlaceApi.Domain;
using System.ComponentModel.DataAnnotations;

namespace JobMarketPlaceApi.Entities
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        // Pure factory: enforces domain invariants and returns a Job instance ready to persist.
        public Job CreateJob(string description, DateTime startDate, DateTime? dueDate = null, int? budget = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("Description is required");

            var effectiveDue = dueDate ?? startDate;
            if (startDate > effectiveDue)
                throw new DomainException("StartDate must be less than or equal to DueDate");

            var effectiveBudget = budget ?? 0;
            if (effectiveBudget < 0)
                throw new DomainException("Budget must be >= 0");

            return new Job
            {
                Id = Guid.NewGuid(), // factory generates id
                CustomerId = this.Id,
                Description = description.Trim(),
                AcceptedBy = string.Empty,
                Budget = effectiveBudget,
                StartDate = startDate,
                DueDate = effectiveDue
            };
        }
    }
}
