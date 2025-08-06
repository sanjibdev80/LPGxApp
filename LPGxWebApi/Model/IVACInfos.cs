using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
	public class IVACInfos
	{
		[Key]
		public int ID { get; set; }

		[Display(Name = "IVAC Location")]
		[StringLength(100, MinimumLength = 2)]
		public string? LOCATIONS { get; set; }

		[Display(Name = "Visa Type")]
		[StringLength(50, MinimumLength = 2)]
		public string? VISATYPE { get; set; }

		[Display(Name = "Web File Number")]
		[StringLength(50, MinimumLength = 5)]
		public string? WEBNO { get; set; }

		[Display(Name = "Full Name")]
		[StringLength(100, MinimumLength = 2)]
		public string? FULLNAME { get; set; }

		[Display(Name = "Date of Birth")]
		public DateTime? DOB { get; set; } = DateTime.Now;

		[Display(Name = "Email")]
		[EmailAddress]
		[StringLength(100, MinimumLength = 5)]
		public string? EMAIL { get; set; }

		[Display(Name = "Email Password")]
		[StringLength(50, MinimumLength = 4)]
		public string? EMAILPIN { get; set; }

		[Display(Name = "Phone (OTP Verify)")]
		[Phone]
		[StringLength(15, MinimumLength = 7)]
		public string? PHONE { get; set; }

		[Display(Name = "IVAC Login Id")]
		[StringLength(50, MinimumLength = 3)]
		public string? LID { get; set; }

		[Display(Name = "IVAC Registered Password")]
		[StringLength(50, MinimumLength = 4)]
		public string? PIN { get; set; }

		[Display(Name = "Purpose of Visit (Maximum 52 characters)")]
		[StringLength(52, MinimumLength = 2)]
		public string? PURPOSE { get; set; }

		// Attendees
		[Display(Name = "Web File Number")]
		public string? A1WEBNO { get; set; }

		[Display(Name = "Full Name")]
		public string? A1NAME { get; set; }

		[Display(Name = "Web File Number")]
		public string? A2WEBNO { get; set; }

		[Display(Name = "Full Name")]
		public string? A2NAME { get; set; }

		[Display(Name = "Web File Number")]
		public string? A3WEBNO { get; set; }

		[Display(Name = "Full Name")]
		public string? A3NAME { get; set; }

		[Display(Name = "Web File Number")]
		public string? A4WEBNO { get; set; }

		[Display(Name = "Full Name")]
		public string? A4NAME { get; set; }

		// Payment Info
		[Display(Name = "Payment Type")]
		[StringLength(30, MinimumLength = 2)]
		public string? PAYTYPE { get; set; }

		[Display(Name = "Payment Number")]
		[StringLength(15, MinimumLength = 7)]
		public string? PAYPHONE { get; set; }

		[Display(Name = "Current Status")]
		public string? REMARKS { get; set; }

		// Audit Info
		public int? ENTRYUSER { get; set; }

		public DateTime? CREATEDATE { get; set; }

		[StringLength(1)]
		public string? ENABLEYN { get; set; } // Y/N
	}

}
