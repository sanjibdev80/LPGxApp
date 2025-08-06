using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class SaleInfos
    {
        [Key] // Define SaleId as the primary key
        public long SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public long PersonId { get; set; }
        public long ProductId { get; set; }
        public int Unit { get; set; }
        public int Packages { get; set; }
        public int CylinderDue { get; set; }
        public int CylinderPaid { get; set; }
        public int OnDate { get; set; }
        public int CylinderExchange { get; set; }
        public int LoanPaid { get; set; }
        public int CylinderSalePurchase { get; set; }
        public int Today { get; set; }
        public int PreviousStock { get; set; }
        public int RefilePurchase { get; set; }
        public int PackagePurchase { get; set; }
        public int TotalStock { get; set; }
        public string? BranchCode { get; set; }
    }
}
