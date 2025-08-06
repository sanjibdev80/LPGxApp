using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class GenerateOTP
    {
        [Key] // Marks REQID as the primary key
        public decimal REQID { get; set; } // This will be auto-generated

        [Required] // Makes SIGNONID a required field
        [StringLength(15)] // Validates the length
        public string SIGNONID { get; set; }

        [StringLength(50)] // Validates the length
        public string? OTP { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public DateTime? EXPIRY { get; set; }
        public bool VERIFYOTP { get; set; }
        public DateTime? VERIFYTIME { get; set; }
        public string? DATAARRAY { get; set; }

    }
}
