namespace LPGxWebApp.Response
{
    public class SalesmanResponse
    {
        public long PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? ContactNo { get; set; }
        public string? Address { get; set; }
        public string? ActiveYN { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; } = null;
    }
}
