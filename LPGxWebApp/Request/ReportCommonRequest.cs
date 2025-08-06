namespace LPGxWebApp.Request
{
    public class ReportCommonRequest
    {
        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public string? BRANCHCODE { get; set; }
        public string? SIGNONNAME { get; set; }
    }
}
