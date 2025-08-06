using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class LevelInfos
    {
        [Key] // Define USERLEVEL as the primary key
        public int USERLEVEL { get; set; }
        public string LEVELNAME { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
