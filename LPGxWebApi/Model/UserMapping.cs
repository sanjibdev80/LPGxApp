using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class UserMapping
    {
        [Key] // Define MAPPINGID as the primary key
        public int MAPPINGID { get; set; }
        public int USERCODE { get; set; }
        public string BRANCHCODE { get; set; }
        public string PROJECTCODE { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
