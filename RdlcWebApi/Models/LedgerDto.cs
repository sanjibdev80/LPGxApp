using System;

namespace RdlcgWebApi.ReportDataTable
{
    public class LedgerDto
    {
        public string? AptNo { get; set; }               // Apartment number
        public string? Name { get; set; }               // Apartment number
        public string? Particular { get; set; }         // Transaction type name (e.g., BILL, COLLECTION, OPENING B/F)
        public string? TxnType { get; set; }              // BILL, COLLECTION, or OPENING
        public DateTime TxnDate { get; set; }           // Transaction date
        public decimal Bill { get; set; }               // Amount if it is a BILL
        public decimal Collection { get; set; }         // Amount if it is a COLLECTION
        public decimal DueBalance { get; set; }         // Running balance
    }
}
