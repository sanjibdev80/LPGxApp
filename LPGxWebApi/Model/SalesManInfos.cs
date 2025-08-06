using System.ComponentModel.DataAnnotations;

namespace LPGxWebApi.Model
{
    public class SalesManInfos
    {
        [Key] // Define PersonId as the primary key
        public long PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? ContactNo { get; set; }
        public string? Address { get; set; }
        public string? ActiveYN { get; set; }
        public string? BranchCode { get; set; }

    }
}
