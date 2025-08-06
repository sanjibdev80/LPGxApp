using System;

namespace RdlcgWebApi.ReportDataTable
{
    public class DailyDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string TxnRef { get; set; }
        public string VoucherRef { get; set; }
        public string Particular { get; set; }
        public decimal Amount { get; set; }
        public string AptNo { get; set; }
        public int SlNo { get; set; }
    }
}
