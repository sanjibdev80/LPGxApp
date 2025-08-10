using LPGxWebApp.Class;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LPGxWebApp.Pages.Login
{
    public class LoginIndexModel : PageModel
    {
        [BindProperty]
        public string CountryCode { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        private readonly ILogger<LoginIndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        public LoginResponse LoginInfo { get; set; }  // Holds the response data

        public LoginIndexModel(ILogger<LoginIndexModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> CallApiAsync(string countryCode, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogWarning("Phone Number is missing");
                return false;
            }

            try
            {
                // Get the API BaseUrl from the configuration
                string baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    TempData["Message"] = $"API BaseUrl is not configured";
                    TempData["MessageType"] = "Error";
                    _logger.LogError("API BaseUrl is not configured");
                    return false;
                }

                // Construct the full API URL
                string apiUrl = $"{baseUrl}LoginInfos/Login/signon/{countryCode}{phoneNumber}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);

                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    TempData["Message"] = $"No response from API for userId: {countryCode}{phoneNumber}";
                    TempData["MessageType"] = "Error";
                    _logger.LogWarning($"No response from API for userId: {countryCode}{phoneNumber}");
                    return false;
                }

                // Deserialize the response to LoginResponse
                LoginInfo = JsonConvert.DeserializeObject<LoginResponse>(response);

                if (LoginInfo == null)
                {
                    TempData["Message"] = $"Failed to deserialize response for userId: {countryCode}{phoneNumber}";
                    TempData["MessageType"] = "Error";
                    _logger.LogError($"Failed to deserialize response for userId: {countryCode}{phoneNumber}");
                    return false;
                }

                _logger.LogInformation($"Login data successfully retrieved for userId: {countryCode}{phoneNumber}");
                return true;
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error calling API: {ex.Message}";
                TempData["MessageType"] = "Error";
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Handle GET request
        public async Task OnGetAsync([FromQuery] string countryCode, [FromQuery] string phoneNumber)
        {
            if (await CallApiAsync(countryCode, phoneNumber))
            {
                _logger.LogInformation("API call successful during OnGetAsync.");
            }
            else
            {
                _logger.LogError("API call failed during on loading data");
            }
        }

        // Handle POST request
        public async Task<IActionResult> OnPostAsync()
        {
            if (await CallApiAsync(CountryCode, PhoneNumber))
            {
                VerifyLoginRoute routes = new VerifyLoginRoute();
                routes.SignonName = CountryCode + PhoneNumber;
                routes.ReqId = LoginInfo.ReqId;
                routes.TwoFA = LoginInfo.TwoFA;

                // Store in TempData instead of passing via query string
                TempData["SignonName"] = routes.SignonName;
                TempData["ReqId"] = routes.ReqId;
                TempData["TwoFA"] = routes.TwoFA;

                if (LoginInfo != null && LoginInfo.TwoFA == true)
                {
                    return RedirectToPage("VerifyOTP"); // Redirect on success
                }
                else
                {
                    return RedirectToPage("VerifyPassword"); // Redirect on success
                }
            }

            TempData["Message"] = $"Invalid login request or API call failed during operation";
            TempData["MessageType"] = "Error";
            _logger.LogError("Invalid login request or API call failed during operation");
            return Page(); // Stay on the same page if something went wrong
        }
 
    }
}

