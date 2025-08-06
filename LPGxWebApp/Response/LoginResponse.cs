namespace LPGxWebApp.Response
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public int ReqId { get; set; }
        public bool TwoFA { get; set; }
    }
}

