namespace LPGxWebApp.Response
{
    public class UserMappingResponse
    {
        public int MAPPINGID { get; set; }  // Correct: MAPPINGID is an integer
        public int USERCODE { get; set; }  // Correct: USERCODE is a string
        public string? USERNAME { get; set; }  // Nullable string (for USERNAME from LOGIN)
        public int? USERLEVEL { get; set; }  // Nullable string (for USERNAME from LOGIN)
        public string? SIGNONID { get; set; }  // Nullable string (for SIGNONID from LOGIN)
        public string? EMAILID { get; set; }  // Nullable string (for EMAILID from LOGIN)
        public string BRANCHCODE { get; set; }  // Correct: BLDCODE is a string
        public string? BRANCHNAME { get; set; }  // Nullable string (for BLDNAME from BLDINFO)
        public string PROJECTCODE { get; set; }  // Correct: PROJECTCODE is a string
        public string? PROJECTNAME { get; set; }  // Nullable string (for PROJECTNAME from PROJECTINFO)
        public int? ENTRYUSER { get; set; }  // Correct: ENTRYUSER is a string
        public DateTime? CREATEDATE { get; set; }  // Correct: DateTime, but nullable DateTime handled in LINQ
        public string ENABLEYN { get; set; }  // Correct: ENABLEYN is a string
    }
}
