namespace LPGxWebApp.Response
{
    public class UserListResponse
    {
        public int USERCODE { get; set; }
        public string USERNAME { get; set; }
        public string SIGNONID { get; set; }
        public int USERLEVEL { get; set; }
        public int AUTHENTICATORID { get; set; }
        public string? EMAILID { get; set; }
        public bool TWOFA { get; set; }
        public string? PASSWORD { get; set; }
        public int? ENTRYUSER { get; set; }
        public DateTime? CREATEDATE { get; set; }
        public string? ENABLEYN { get; set; }
    }
}
