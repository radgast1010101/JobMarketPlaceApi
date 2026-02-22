namespace JobMarketPlaceApi.Entities
{
    public class Job
    {
        // Owner
        public Guid CustomerId { get; set; }
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string AcceptedBy { get; set; }
        public int Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

    }
}
