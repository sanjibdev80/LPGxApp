using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Request;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LPGxWebApp.Pages
{
    public class SaleModel : PageModel
    {
        private readonly ILogger<SaleModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the Sale data
        public List<SaleReportResponse> Sales { get; set; }
        public List<SalesmanResponse> Salesman { get; set; }
        public List<ProductResponse> Distributors { get; set; }

        [BindProperty]
        public ProductForm ProductsForm { get; set; }

        public SaleModel(ILogger<SaleModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> ReportCallApiAsync(string BranchCode, string SignonName)
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

                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Construct the full API URL
                string apiUrl = $"{baseUrl}SaleInfos/Date/{BranchCode}/{currentDate}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
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
                Sales = SaleResponse;

                _logger.LogInformation($"Sale data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Consolidated method to handle API calls
        private async Task<bool> DistributorCallApiAsync(string BranchCode, string SignonName)
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

                // Construct the full API URL
                string apiUrl = $"{baseUrl}ProductInfos/Distributor/{BranchCode}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to aptNoResponse
                var jsonObject = JObject.Parse(response);
                var aptNoJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var distResponse = JsonConvert.DeserializeObject<List<ProductResponse>>(aptNoJsonArray);
                if (distResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                Distributors = distResponse;

                _logger.LogInformation($"Distributor List data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Consolidated method to handle API calls
        private async Task<bool> SalesmanCallApiAsync(string BranchCode, string SignonName)
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

                // Construct the full API URL
                string apiUrl = $"{baseUrl}SalesManInfos/Branches/{BranchCode}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to aptNoResponse
                var jsonObject = JObject.Parse(response);
                var aptNoJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var manResponse = JsonConvert.DeserializeObject<List<SalesmanResponse>>(aptNoJsonArray);
                if (manResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                Salesman = manResponse;

                _logger.LogInformation($"Salesman data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Consolidated method to handle API calls
        private async Task<bool> FormPostCallApiAsync()
        {
            try
            {
                // Get the API BaseUrl from the configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("API BaseUrl is not configured");
                    TempData["Message"] = $"API BaseUrl is not configured";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}SaleInfos";

                // Prepare the request body
                var requestBody = new
                {
                    SaleId = ProductsForm.SaleId,
                    SaleDate = ProductsForm.SaleDate,
                    PersonId = ProductsForm.PersonId,
                    ProductId = ProductsForm.ProductId,
                    Unit = ProductsForm.Unit,
                    Packages = ProductsForm.Packages,
                    CylinderDue = ProductsForm.CylinderDue,
                    CylinderPaid = ProductsForm.CylinderPaid,
                    OnDate = ProductsForm.OnDate,
                    CylinderExchange = ProductsForm.CylinderExchange,
                    LoanPaid = ProductsForm.LoanPaid,
                    CylinderSalePurchase = ProductsForm.CylinderSalePurchase,
                    Today = ProductsForm.Today,
                    PreviousStock = ProductsForm.PreviousStock,
                    RefilePurchase = ProductsForm.RefilePurchase,
                    PackagePurchase = ProductsForm.PackagePurchase,
                    TotalStock = ProductsForm.TotalStock,
                    BranchCode = ProductsForm.BranchCode,
                };

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, requestBody);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    TempData["Message"] = $"No response from API";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Initialize jsonObject for parsing the response
                JObject jsonObject = null;

                try
                {
                    // Attempt to parse the response into a JObject
                    jsonObject = JObject.Parse(response);
                }
                catch (JsonException ex)
                {
                    // Handle the case where parsing fails (response isn't valid JSON)
                    _logger.LogError($"Failed to parse JSON response: {ex.Message}. Raw response: {response}");
                    TempData["Message"] = response;
                    TempData["MessageType"] = "Error";
                    await OnGetAsync();
                    return false;
                }

                // Check if the response contains data
                if (jsonObject == null)
                {
                    _logger.LogWarning("Parsed JSON is null.");
                    TempData["Message"] = "No valid data was received in the response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Extract the "message" field from the response
                var messageJsonData = jsonObject["message"]?.ToString();

                if (string.IsNullOrEmpty(messageJsonData))
                {
                    // Log and notify if "message" field is missing or empty
                    _logger.LogError("Response contains no 'message' field.");
                    TempData["Message"] = "No 'message' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Log the message if it's available
                _logger.LogInformation($"API Response Message: {messageJsonData}");

                // Extract the "data" field from the response
                var data = jsonObject["data"];
                if (data == null)
                {
                    // Log and notify if "data" field is missing or null
                    _logger.LogError("Response contains no 'data' field.");
                    TempData["Message"] = "No 'data' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Extract specific data fields from the "data" object
                var Salename = data["saleId"]?.ToString();
                if (string.IsNullOrEmpty(Salename))
                {
                    // Log and notify if "Salename" is missing or empty
                    _logger.LogError("Response contains no 'Salename' field.");
                    TempData["Message"] = "No 'Salename' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Log the successful response with the Building code
                _logger.LogInformation($"TrackId: {Salename}. Data inserted successfully");

                // Set a success message for the user
                TempData["Message"] = $"TrackId: {Salename}. Data inserted successfully";
                TempData["MessageType"] = "Success";

                await OnGetAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                TempData["Message"] = $"Error calling API: {ex.Message}";
                TempData["MessageType"] = "Error";
                return false;
            }
        }

        // OnGet method to call the API when the page is accessed
        public async Task<IActionResult> OnGetAsync()
        {
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

            bool successAptNoList = await DistributorCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName);
            if (!successAptNoList)
            {
                _logger.LogError("Failed to retrieve report.");
                return Page(); // Stay on the same page if API call fails
            }

            bool successManList = await SalesmanCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName);
            if (!successManList)
            {
                _logger.LogError("Failed to retrieve report.");
                return Page(); // Stay on the same page if API call fails
            }

            bool success = await ReportCallApiAsync(verifyLoginData.BranchCode, verifyLoginData.SignonName);
            if (!success)
            {
                _logger.LogError("Failed to retrieve projects.");
                return Page(); // Stay on the same page if API call fails
            }

            return Page(); // Proceed to render the page if data is successfully retrieved
        }

        // Consolidated method to handle API calls
        public async Task<IActionResult> OnPostAsync()
        {
            bool successReport = await FormPostCallApiAsync();

            if (!successReport)
            {
                _logger.LogError("Failed to retrieve save data.");
                return Page(); // Stay on the same page if API call fails
            }

            return Page(); // Proceed to render the page if data is successfully retrieved
        }

        public class ProductForm
        {
            public long SaleId { get; set; }
            public DateTime SaleDate { get; set; }
            public long PersonId { get; set; }
            public long ProductId { get; set; }
            public int Unit { get; set; }
            public int Packages { get; set; }
            public int CylinderDue { get; set; }
            public int CylinderPaid { get; set; }
            public int OnDate { get; set; }
            public int CylinderExchange { get; set; }
            public int LoanPaid { get; set; }
            public int CylinderSalePurchase { get; set; }
            public int Today { get; set; }
            public int PreviousStock { get; set; }
            public int RefilePurchase { get; set; }
            public int PackagePurchase { get; set; }
            public int TotalStock { get; set; }
            public string? BranchCode { get; set; }

        }

    }
}