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
    public class DistributorsModel : PageModel
    {
        private readonly ILogger<DistributorsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the Distributors data
        public List<ProductResponse> Distributors { get; set; }
        public List<ProductResponse> ProductList { get; set; }

        [BindProperty]
        public ProductForm ProductsForm { get; set; }

        public DistributorsModel(ILogger<DistributorsModel> logger, IConfiguration configuration, ApiService apiService)
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

                // Construct the full API URL
                string apiUrl = $"{baseUrl}ProductInfos/Distributor/{BranchCode}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to DistributorsResponse
                var jsonObject = JObject.Parse(response);
                var DistributorssJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var DistributorsResponse = JsonConvert.DeserializeObject<List<ProductResponse>>(DistributorssJsonArray);

                if (DistributorsResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                Distributors = DistributorsResponse;

                _logger.LogInformation($"Distributors data successfully retrieved");
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
                string apiUrl = $"{baseUrl}ProductInfos/Branches/{BranchCode}";

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
                var aptNoResponse = JsonConvert.DeserializeObject<List<ProductResponse>>(aptNoJsonArray);

                if (aptNoResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                ProductList = aptNoResponse;

                _logger.LogInformation($"AptNo List data successfully retrieved");
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
                string apiUrl = $"{baseUrl}ProductInfos/Distributor/Mark";

                // Prepare the request body
                var requestBody = new
                {
                    ProductId = ProductsForm.ProductId,
                    DistributorYN = ProductsForm.DistributorYN,
                    BranchCode = ProductsForm.BranchCode,
                };

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("PUT", apiUrl, requestBody);
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
                var Distributorsname = data["productName"]?.ToString();
                if (string.IsNullOrEmpty(Distributorsname))
                {
                    // Log and notify if "Distributorsname" is missing or empty
                    _logger.LogError("Response contains no 'Distributorsname' field.");
                    TempData["Message"] = "No 'Distributorsname' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Log the successful response with the Building code
                _logger.LogInformation($"{Distributorsname} registered successfully");

                // Set a success message for the user
                TempData["Message"] = $"{Distributorsname} registered successfully";
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
            public long ProductId { get; set; }
            public string? DistributorYN { get; set; }
            public string? BranchCode { get; set; }

        }

    }
}