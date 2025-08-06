using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class BranchInfos
    {
        [Key] // Define BRANCHCODE as the primary key
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
