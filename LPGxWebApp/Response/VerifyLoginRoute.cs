namespace LPGxWebApp.Response
{
    public class VerifyLoginRoute
    {
        public string? SignonName { get; set; }
        public int ReqId { get; set; }
        public bool TwoFA { get; set; }
    }
}
