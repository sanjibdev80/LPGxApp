using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Request;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Web.WebPages;

namespace LPGxWebApp.Pages.Report
{
    public class DailyStockReportModel : PageModel
    {
        private readonly ILogger<DailyStockReportModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the ReturnsReport data
        public List<ReturnReportResponse> ReturnReport { get; set; }
        public List<SaleReportResponse> SalesReport { get; set; }
        public List<SalesReturnResponseDto> DailyStockReport { get; set; }

        [BindProperty]
        public DateTime reportDate { get; set; }

        public DailyStockReportModel(ILogger<DailyStockReportModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> ReturnReportCallApiAsync(string BranchCode, string SignonName, DateTime reportDate)
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

        // Consolidated method to handle API calls
        private async Task<bool> SalesReportCallApiAsync(string BranchCode, string SignonName, DateTime reportDate)
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
                string apiUrl = $"{baseUrl}SaleInfos/Date/{BranchCode}/{rptDate.ToString("yyyy-MM-dd")}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No data found");
                    TempData["Message"] = $"No data found";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Deserialize the response to SaleResponse
                var jsonObject = JObject.Parse(response);
                var SalesJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var SaleResponse = JsonConvert.DeserializeObject<List<SaleReportResponse>>(SalesJsonArray);

                if (SaleResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                SalesReport = SaleResponse;

                _logger.LogInformation($"Sales Report data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Consolidated method to handle API calls
        private async Task<bool> PDFCallApiAsync(string BranchCode, string signonName, DateTime reportDate)
        {
            try
            {
                // Get the API BaseUrl from the configuration
                string reportUrl = _configuration.GetValue<string>("ApiSettings:ReportUrl");
                if (string.IsNullOrWhiteSpace(reportUrl))
                {
                    _logger.LogError("API ReportUrl is not configured.");
                    TempData["ReportPDFBaseData"] = null;
                    TempData["Message"] = "API URL is not configured.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Construct the full API URL
                string apiUrl = $"{reportUrl}LPGxReport/SalesReturnRegister";

                var request = new LPGxReportParamRequest
                {
                    RPTDATE = reportDate,
                    BRANCHCODE = BranchCode,
                    SIGNONNAME = signonName
                };

                // Call the API
                var response = await _apiService.CallApiAsync("POST", apiUrl, request);

                if (string.IsNullOrWhiteSpace(response) || response.Contains("No record found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("No data found in API response.");
                    TempData["ReportPDFBaseData"] = null;
                    TempData["Message"] = "No data found.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Try parsing the JSON response
                JObject jsonObject;
                try
                {
                    jsonObject = JObject.Parse(response);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Failed to parse JSON response: {ex.Message}. Raw response: {response}");
                    TempData["ReportPDFBaseData"] = null;
                    TempData["Message"] = "Invalid JSON response from API.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Extract and validate "message"
                string message = jsonObject["message"]?.ToString();
                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogError("Response missing 'message' field.");
                    TempData["ReportPDFBaseData"] = null;
                    TempData["Message"] = "Missing 'message' in API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }
                _logger.LogInformation($"API Message: {message}");

                // Extract and validate "data"
                var data = jsonObject["data"];
                if (data == null || string.IsNullOrWhiteSpace(data.ToString()))
                {
                    _logger.LogError("Response missing or empty 'data' field.");
                    TempData["ReportPDFBaseData"] = null;
                    TempData["Message"] = "Missing 'data' in API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Store PDF base64 data in TempData
                TempData["ReportPDFBaseData"] = data.ToString();
                TempData["Message"] = message;
                TempData["MessageType"] = "Success";

                _logger.LogInformation("PDF data successfully retrieved and stored.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during PDF report API call.");
                TempData["ReportPDFBaseData"] = null;
                TempData["Message"] = "Unexpected error occurred while generating report.";
                TempData["MessageType"] = "Error";
                return false;
            }
        }


        // OnGet method to call the API when the page is accessed
        public async Task<IActionResult> OnGetAsync()
        {
            TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");

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

            bool successReturn = await ReturnReportCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName, rptDate);
            if (!successReturn)
            {
                TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");

                _logger.LogError("Failed to retrieve projects.");
                OnGetAsync();
                Thread.Sleep(1000);
                return Page(); // Stay on the same page if API call fails
            }

            bool successSales = await SalesReportCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName, rptDate);
            if (!successSales)
            {
                TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");

                _logger.LogError("Failed to retrieve projects.");
                OnGetAsync();
                Thread.Sleep(1000);
                return Page(); // Stay on the same page if API call fails
            }

            // Call the SalesReturnReport API
            var salesReturnList = new List<SalesReturnResponseDto>();
            foreach (var sale in SalesReport)
            {
                var ret = ReturnReport.FirstOrDefault(r => r.Name == sale.Name);

                var combined = new SalesReturnResponseDto
                {
                    NAME = sale.Name,
                    RPTDATE = rptDate,
                    NAV = sale.NAV,
                    DUB = sale.DUB,
                    UNI = sale.UNI,
                    TOTAL = sale.TOT,
                    PKG = sale.PKG,
                    CD = sale.CD,
                    CP = sale.CP,
                    DT = sale.DT,
                    TOTAL1 = sale.TOTAL,

                    // Return side
                    NAV_R = ret?.NAV,
                    DUB_R = ret?.DUB,
                    UNI_R = ret?.UNI,
                    OME = ret?.OME,
                    MAX = ret?.MAX,
                    FRESH = ret?.FRESH,
                    KH = ret?.KH,
                    BM = ret?.BM,
                    DEL = ret?.DEL,
                    JAM = ret?.JAM,
                    BEX = ret?.BEX,
                    BEN = ret?.BEN,
                    SEN = ret?.SEN,
                    BAS = ret?.BAS,
                    JMI = ret?.JMI,
                    GGAS = ret?.GGAS,
                    TMSS = ret?.TMSS,
                    AY = ret?.AY,
                    ETC = ret?.ETC,
                    TOTAL2 = ret?.NAV + ret?.DUB + ret?.UNI + ret?.OME + ret?.MAX + ret?.FRESH + ret?.KH + ret?.BM + ret?.DEL + ret?.JAM + ret?.BEX + ret?.BEN + ret?.SEN + ret?.BAS + ret?.JMI + ret?.GGAS + ret?.TMSS + ret?.AY + ret?.ETC
                };

                salesReturnList.Add(combined);
            }
            DailyStockReport = salesReturnList;

            // Call the PDF API
            bool successPdf = await PDFCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName, rptDate);

            TempData["ReportDate"] = reportDate != DateTime.MinValue ? reportDate.ToString("dd-MMM-yyyy") : DateTime.Now.ToString("dd-MMM-yyyy");
            OnGetAsync();
            Thread.Sleep(1000);
            return Page(); // Proceed to render the page if data is successfully retrieved
        }

        public class SalesReturnResponseDto
        {
            public string? NAME { get; set; }
            public DateTime? RPTDATE { get; set; }
            public int? NAV { get; set; }
            public int? DUB { get; set; }
            public int? UNI { get; set; }
            public int? TOTAL { get; set; }
            public int? PKG { get; set; }
            public int? CD { get; set; }
            public int? CP { get; set; }
            public int? DT { get; set; }
            public int? TOTAL1 { get; set; }

            public int? NAV_R { get; set; }
            public int? DUB_R { get; set; }
            public int? UNI_R { get; set; }
            public int? OME { get; set; }
            public int? MAX { get; set; }
            public int? FRESH { get; set; }
            public int? KH { get; set; }
            public int? BM { get; set; }
            public int? DEL { get; set; }
            public int? JAM { get; set; }
            public int? BEX { get; set; }
            public int? BEN { get; set; }
            public int? SEN { get; set; }
            public int? BAS { get; set; }
            public int? JMI { get; set; }
            public int? GGAS { get; set; }
            public int? TMSS { get; set; }
            public int? AY { get; set; }
            public int? ETC { get; set; }
            public int? TOTAL2 { get; set; }
        }

        public class LPGxReportParamRequest
        {
            public DateTime? RPTDATE { get; set; }
            public string? BRANCHCODE { get; set; }
            public string? SIGNONNAME { get; set; }

        }
    }
}