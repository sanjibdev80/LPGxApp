namespace LPGxWebApi.ViewModel
{
    public class SuccessfulLoginModel
    {
        public string UserName { get; set; }
        public string SignonName { get; set; }
        public int UserLevel { get; set; }
        public int? UserCode { get; set; }
        public string? Email { get; set; }
        public string? ProjectCode { get; set; }
        public string? BranchCode { get; set; }
        public string? CountryCode { get; set; }
        public bool? TwoFA { get; set; }
    }
}
