namespace LPGxWebApp.Request
{
    public class VerifyPasswordRequest
    {
        public string SignonName { get; set; }
        public string Password { get; set; }
        public decimal ReqId { get; set; }
    }
}
