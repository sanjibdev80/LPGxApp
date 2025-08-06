using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class ProductInfos
    {
        [Key] // Define ProductId as the primary key
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDetails { get; set; }
        public string? ActiveYN { get; set; }
        public string? DistributorYN { get; set; }
        public string? BranchCode { get; set; }
    }
}
