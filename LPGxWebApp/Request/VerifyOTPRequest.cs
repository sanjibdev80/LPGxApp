namespace LPGxWebApp.Request
{
    public class VerifyOTPRequest
    {
        public string SignonName { get; set; }
        public string OtpCode { get; set; }
        public decimal ReqId { get; set; }
    }
}
