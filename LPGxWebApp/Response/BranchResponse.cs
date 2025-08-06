namespace LPGxWebApp.Response
{
    public class BranchResponse
    {
        public string BRANCHCODE { get; set; }
        public string PROJECTCODE { get; set; }
        public string BLDNAME { get; set; }
        public string? BLDADDRESS { get; set; }
        public string? BLDCITY { get; set; }
        public string? COUNTRYCODE { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
