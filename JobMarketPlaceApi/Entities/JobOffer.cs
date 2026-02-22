namespace JobMarketPlaceApi.Entities
{
    public class JobOffer
    {
        public Guid Id { get; set; }

        // The job this offer targets
        public Guid JobId { get; set; }

        // Who created the offer (contractor)
        public Guid ContractorId { get; set; }
        public int Price { get; set; }
    }
}
