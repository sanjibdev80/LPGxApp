namespace LPGxWebApp.Response
{
    public class ReturnResponse
    {
        public long ReturnSaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public long PersonId { get; set; }
        public long ProductId { get; set; }
        public int Unit { get; set; }
        public int CylinderExchange { get; set; }
        public int LoanPaid { get; set; }
        public int CylinderSalePurchase { get; set; }
        public int Today { get; set; }
        public int PreviousStock { get; set; }
        public int RefilePurchase { get; set; }
        public int PackagePurchase { get; set; }
        public int TotalStock { get; set; }
        public string? BranchCode { get; set; }
        public string? PersonName { get; set; }
        public string? ProductName { get; set; }
    }
}
