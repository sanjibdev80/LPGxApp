using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class AuthenticatorInfos
    {
        [Key] // Define AUTHENTICATORID as the primary key
        public int AUTHENTICATORID { get; set; }
        public string? AUTHNAME { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}

