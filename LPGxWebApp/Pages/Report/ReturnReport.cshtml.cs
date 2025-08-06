using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Request;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;

namespace LPGxWebApp.Pages.Report
{
    public class ReturnReportModel : PageModel
    {
        private readonly ILogger<ReturnReportModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the ReturnsReport data
        public List<ReturnReportResponse> ReturnReport { get; set; }

        [BindProperty]
        public DateTime reportDate { get; set; }
        
        public ReturnReportModel(ILogger<ReturnReportModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> ReportCallApiAsync(string BranchCode, string SignonName, DateTime reportDate)
        {
            try
            {
                // Get the API BaseUrl from the configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("API BaseUrl is not configured.");
                    return false;
                }

                DateTime rptDate = reportDate.Date;  // Removes the time component

                // Construct the full API URL
                string apiUrl = $"{baseUrl}ReturnInfos/Date/{BranchCode}/{rptDate.ToString("yyyy-MM-dd")}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No data found");
                    TempData["Message"] = $"No data found";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Deserialize the response to ReturnResponse
                var jsonObject = JObject.Parse(response);
                var ReturnsJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var ReturnResponse = JsonConvert.DeserializeObject<List<ReturnReportResponse>>(ReturnsJsonArray);

                if (ReturnResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                ReturnReport = ReturnResponse;

                _logger.LogInformation($"Returns Report data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // OnGet method to call the API when the page is accessed
        public async Task<IActionResult> OnGetAsync()
        {
            TempData["ReportDate"] = reportDate != DateTime.MinValue? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");

            // Retrieve the VerifyLoginData from session
            var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");
            if (verifyLoginData == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Retrieve the branchname from session
            var branchname = HttpContext.Session.GetObject<string>("branchname");
            if (branchname == null)
            {
                return RedirectToPage("/Login/Index");
            }
            TempData["branchname"] = branchname;

            return Page(); // Proceed to render the page if data is successfully retrieved
        }

        // OnPost method to call the API when the page is accessed
        public async Task<IActionResult> OnPostAsync()
        {
            // Set the time to the beginning of the day for starttime
            DateTime rptDate = reportDate.Date;  // Removes the time component

            // Retrieve the VerifyLoginData from session
            var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");
            if (verifyLoginData == null)
            {
                return RedirectToPage("/Login/Index");
            }

            bool success = await ReportCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName, rptDate);

            if (!success)
            {
                TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");

                _logger.LogError("Failed to retrieve projects.");
                OnGetAsync();
                Thread.Sleep(1000);
                return Page(); // Stay on the same page if API call fails
            }

            TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");
            OnGetAsync();
            Thread.Sleep(1000);
            return Page(); // Proceed to render the page if data is successfully retrieved
        }

    }
}