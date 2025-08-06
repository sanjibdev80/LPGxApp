using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Request;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LPGxWebApp.Pages.ProjectsModel;

namespace LPGxWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        public List<UserListResponse> UserList { get; set; }
        public List<BranchResponse> BranchList { get; set; }

        [BindProperty]
        public string bldcodeswitch { get; set; }
        [BindProperty]
        public string projectcodeswitch { get; set; }
        [BindProperty]
        public string countrycodeswitch { get; set; }


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Set the time to the beginning of the day for starttime
            DateTime startDate = System.DateTime.Now.Date.AddDays(-360);  // Removes the time component

            // Set the time to the end of the day for endtime
            DateTime endDate = System.DateTime.Now.Date.AddDays(1).AddTicks(-1);  // Sets to the last tick of the day (23:59:59.9999999)

            // Retrieve the VerifyLoginData from session
            var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");

            // If no login data found, redirect to the login page
            if (verifyLoginData == null)
            {
                return RedirectToPage("/Login/index"); // Or /Login depending on your logic
            }

            //------- Nav Bar data Call------
            bool success = await BranchCallApiAsync(verifyLoginData.SignonName);
            if (!success)
            {
                return Page(); // Stay on the same page if API call fails
            }

            var branchName = BranchList.Where(a => a.BRANCHCODE == verifyLoginData.BranchCode).Select(a => a.BLDNAME).First();

            // Create an instance of RequiredData
            var requiredData = new RequiredData
            {
                BranchName = branchName
            };

            // Store the object in session
            HttpContext.Session.SetObject("branchname", requiredData.BranchName);

            TempData["BranchName"] = requiredData.BranchName;
            TempData["Branches"] = BranchList;
            //------- >>>>>>>>>>>>>>> ------

            //------- User Report Call------
            bool successUserReport = await UserListCallApiAsync(verifyLoginData.BranchCode);
            
            //------- >>>>>>>>>>>>>>> ------

            // Continue with the logic if login data exists
            return Page();  // Or another return statement based on your logic

        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool successReport = await FormPostCallApiAsync(bldcodeswitch, projectcodeswitch, countrycodeswitch);
            if (!successReport)
            {
                _logger.LogError("Failed to retrieve save data.");
                return Page(); // Stay on the same page if API call fails
            }

            // Redirect to ~/Dashboard/Index if the API call is successful
            await OnGetAsync();
            return Page();
        }

        private async Task<bool> FormPostCallApiAsync(string branchcode, string projectcode, string countrycode)
        {
            if (!string.IsNullOrEmpty(branchcode))
            {
                // Create the request from form data
                var request = new UpdateBuildingRequest
                {
                    branchcode = branchcode,
                    projectcode = projectcode,
                    countrycode = countrycode
                };

                // Call the asynchronous method to handle the update and wait for its result
                bool isSuccess = await OnPostUpdateBuilding(request);
                if (isSuccess)
                {
                    // If successful, await OnGetAsync() to fetch updated data
                    return true;
                }
            }

            // If something goes wrong or bldcode is invalid, ensure OnGetAsync() is called
            return false;
        }

        // Consolidated method to handle API calls
        private async Task<bool> BranchCallApiAsync(string signonname)
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
                string apiUrl = $"{baseUrl}BranchInfos/Branches/{signonname}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found") || response.Contains("No profiles found for the provided sign-in ID"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to ApiResponse
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
                if (apiResponse == null || apiResponse.Data == null)
                {
                    _logger.LogError($"Failed to deserialize response or data is missing");
                    return false;
                }

                // Now you have access to the data array
                BranchList = apiResponse.Data;

                _logger.LogInformation($"Login data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        // Consolidated method to handle API calls
        private async Task<bool> UserListCallApiAsync(string branchcode)
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
                string apiUrl = $"{baseUrl}LoginInfos/Login/{branchcode}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to userListResponse
                var userListResponse = JsonConvert.DeserializeObject<List<UserListResponse>>(response);
                if (userListResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                UserList = userListResponse;
                ViewData["UserList"] = UserList;

                _logger.LogInformation($"Login data successfully retrieved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> OnPostUpdateBuilding(UpdateBuildingRequest request)
        {
            try
            {
                // Ensure request is valid
                if (request == null || string.IsNullOrEmpty(request.branchcode))
                {
                    return false;
                }

                // Retrieve session object
                var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");
                if (verifyLoginData == null)
                {
                    return false;
                }

                // Update the session with the selected BldCode
                verifyLoginData.BranchCode = request.branchcode;
                verifyLoginData.ProjectCode = request.projectcode;
                verifyLoginData.CountryCode = request.countrycode;

                // Save it back to the session
                HttpContext.Session.SetObject("VerifyLoginData", verifyLoginData);

                // Optionally call OnGetAsync to refresh any necessary data
                await OnGetAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the error and handle exceptions
                return false;
            }
        }

        public class UpdateBuildingRequest
        {
            public string branchcode { get; set; }
            public string projectcode { get; set; }
            public string countrycode { get; set; }
        }

        public class ApiResponse
        {
            public string Message { get; set; }
            public List<BranchResponse> Data { get; set; }
        }

    }
}