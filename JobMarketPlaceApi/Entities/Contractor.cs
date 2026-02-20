//namespace JobMarketPlaceApi.Entities
//{
//    public class Contractor
//    {
//    }
//}
using System.ComponentModel.DataAnnotations;

namespace JobMarketPlaceApi.Entities
{
    public class Contractor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public int Rating { get; set; }
    }
}
