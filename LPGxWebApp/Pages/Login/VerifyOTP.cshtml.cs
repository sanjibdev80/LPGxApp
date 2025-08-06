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
using LPGxWebApp.Request;
using LPGxWebApp.GlobalData;

namespace LPGxWebApp.Pages.Login
{
    public class LoginVerifyOTPModel : PageModel
    {
        // Access TempData for sensitive data
        public string SignonName { get; set; }
        public int ReqId { get; set; }
        public bool TwoFA { get; set; }

        // OTP Fields
        [BindProperty]
        public string otp1 { get; set; }

        [BindProperty]
        public string otp2 { get; set; }

        [BindProperty]
        public string otp3 { get; set; }

        [BindProperty]
        public string otp4 { get; set; }

        private readonly ILogger<LoginVerifyOTPModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        public VerifyLoginResponse VerifyInfo { get; set; }  // Holds the response data

        public LoginVerifyOTPModel(ILogger<LoginVerifyOTPModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> CallApiAsync(string OTP, int ReqId, string SignonName)
        {
            if (string.IsNullOrEmpty(OTP) || ReqId <= 0 || string.IsNullOrEmpty(SignonName))
            {
                TempData["Message"] = $"OTP, ReqId, or SignonName is missing";
                TempData["MessageType"] = "Error";
                _logger.LogWarning("OTP, ReqId, or SignonName is missing");
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

                // Construct the OTP request payload
                var requestOTP = new VerifyOTPRequest
                {
                    ReqId = ReqId,
                    SignonName = SignonName,
                    OtpCode = OTP
                };

                // Construct the full API URL
                string apiUrl = $"{baseUrl}LoginInfos/VerifyOtp";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("POST", apiUrl, requestOTP);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    TempData["Message"] = $"No response from the API";
                    TempData["MessageType"] = "Error";
                    _logger.LogWarning("No response from the API");
                    return false;
                }

                // Deserialize the response into VerifyLoginResponse
                var VerifyInfo = JsonConvert.DeserializeObject<VerifyLoginResponse>(response);
                if (VerifyInfo == null)
                {
                    TempData["Message"] = $"Failed to deserialize the response";
                    TempData["MessageType"] = "Error";
                    _logger.LogError($"Failed to deserialize the response");
                    return false;
                }

                // Check the deserialized content
                if (VerifyInfo.Data != null)
                {
                    // Store VerifyLoginData in session
                    HttpContext.Session.SetObject("VerifyLoginData", VerifyInfo.Data);
                }

                _logger.LogInformation("API call successful. OTP verified.");
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
        public void OnGet()
        {
            // Retrieve values from TempData
            SignonName = TempData["SignonName"]?.ToString();
            ReqId = (int?)TempData["ReqId"] ?? 0;
            TwoFA = (bool?)TempData["TwoFA"] ?? false;

            TempData["SignonName"] = SignonName;
            TempData["ReqId"] = ReqId;
            TempData["TwoFA"] = TwoFA;

            _logger.LogInformation($"OnGet: SignonName: {SignonName}, ReqId: {ReqId}, TwoFA: {TwoFA}");
        }

        // Handle POST request
        public async Task<IActionResult> OnPostAsync()
        {
            // Concatenate OTP fields into a single OTP string
            var otp = $"{otp1}{otp2}{otp3}{otp4}";

            // Ensure OTP has 4 digits
            if (otp.Length != 4 || otp.Any(c => !char.IsDigit(c)))
            {
                TempData["Message"] = $"Provided OTP is invalid";
                TempData["MessageType"] = "Warning";
                _logger.LogWarning($"Provided OTP is invalid");
                ModelState.AddModelError(string.Empty, "Invalid OTP format.");
                return Page();
            }

            // Retrieve values from TempData
            SignonName = TempData["SignonName"]?.ToString();
            ReqId = (int?)TempData["ReqId"] ?? 0;
            TwoFA = (bool?)TempData["TwoFA"] ?? false;

            TempData["SignonName"] = SignonName;
            TempData["ReqId"] = ReqId;
            TempData["TwoFA"] = TwoFA;

            // Call the API to verify OTP
            if (await CallApiAsync(otp, ReqId, SignonName))
            {
                // Retrieve the VerifyLoginData from session
                var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");

                // Additional logic based on userLevel or other data
                string redirectPage = verifyLoginData.UserLevel switch
                {
                    1 => "/Dashboard/Index", //"SuperAdmin",
                    2 => "/Dashboard/Index", //"Admin",
                    3 => "/Dashboard/Index", //"SalesMan",
                    4 => "/Dashboard/Index", //"Management",
                    _ => "Error" // Default in case no valid UserLevel is found
                };

                if (redirectPage == "Error")
                {
                    TempData["Message"] = $"Unknown UserLevel. Redirecting to error page";
                    TempData["MessageType"] = "Error";
                    _logger.LogWarning("Unknown UserLevel. Redirecting to error page");
                    return RedirectToPage(redirectPage);
                }

                _logger.LogInformation("API call successful during OnPostAsync.");
                return RedirectToPage(redirectPage);

            }

            TempData["Message"] = $"Invalid otp found";
            TempData["MessageType"] = "Error";
            _logger.LogError("Invalid otp found");
            return Page();
        }

    }

}

