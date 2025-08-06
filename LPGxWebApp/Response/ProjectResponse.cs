namespace LPGxWebApp.Response
{
    public class ProjectResponse
    {
        public string PROJECTCODE { get; set; }
        public string COUNTRYCODE { get; set; }
        public string PROJECTNAME { get; set; }
        public string? PROJECTADDRESS { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
