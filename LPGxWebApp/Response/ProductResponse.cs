namespace LPGxWebApp.Response
{
    public class ProductResponse
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDetails { get; set; }
        public string? ActiveYN { get; set; }
        public string? DistributorYN { get; set; }
        public string? BranchCode { get; set; }

    }
}
