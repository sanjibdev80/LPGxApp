using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class ProjectInfos
    {
        [Key] // Define PROJECTCODE as the primary key
        public string PROJECTCODE { get; set; }
        public string COUNTRYCODE { get; set; }
        public string PROJECTNAME { get; set; }
        public string? PROJECTADDRESS { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
