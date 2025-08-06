using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LPGxWebApp.Pages
{
    public class UserMappingModel : PageModel
    {
        private readonly ILogger<UserMappingModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the apartment data
        public List<UserMappingResponse> UserMapping { get; set; }
        public List<BranchResponse> BranchList { get; set; }
        public List<UserListResponse> UserList { get; set; }

        [BindProperty]
        public MappingForm mappingForm { get; set; }

        public UserMappingModel(ILogger<UserMappingModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }

        // Consolidated method to handle API calls
        private async Task<bool> ReportCallApiAsync(string branchcode)
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
                string apiUrl = $"{baseUrl}UserMapping/branch/{branchcode}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to UserMappingResponse
                var UserMappingResponse = JsonConvert.DeserializeObject<List<UserMappingResponse>>(response);

                if (UserMappingResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                UserMapping = UserMappingResponse.Where(u => u.USERLEVEL != 1).ToList();

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
        private async Task<bool> BranchesCallApiAsync(string SignonName)
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
                string apiUrl = $"{baseUrl}BranchInfos/Branches/{SignonName}";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to bldNoResponse
                var jsonObject = JObject.Parse(response);
                var aptNoJsonArray = jsonObject["data"].ToString(); // Extract the "data" array as a string
                var bldNoResponse = JsonConvert.DeserializeObject<List<BranchResponse>>(aptNoJsonArray);

                if (bldNoResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                BranchList = bldNoResponse;

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
        private async Task<bool> UserListCallApiAsync()
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
                string apiUrl = $"{baseUrl}LoginInfos";

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
                UserList = userListResponse.Where(x=>x.USERLEVEL != 1 && x.ENABLEYN == "Y").ToList();

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
        private async Task<bool> FormPostCallApiAsync()
        {
            try
            {
                // Retrieve the VerifyLoginData from session
                var verifyLoginData = HttpContext.Session.GetObject<VerifyLoginData>("VerifyLoginData");
                if (verifyLoginData == null)
                {
                    _logger.LogError("Session data not found");
                    TempData["Message"] = $"Session data not found";
                    TempData["MessageType"] = "Error";
                    return false;
                }

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
                string apiUrl = $"{baseUrl}UserMapping/registration";

                // Split the aptcode string into an array (if needed)
                var branchcodes = mappingForm.BRANCHCODE;  // Assuming aptcode is a comma-separated string

                string[] branchcodeArray = branchcodes.Split(',')
                                .Select(s => s.Trim())  // Trim spaces from each element
                                .ToArray();

                // Prepare the request body
                var requestBody = new
                {
                    usercode = mappingForm.USERCODE,
                    branchcode = branchcodeArray,  // Ensuring bldcode is an array
                    entryuser = verifyLoginData.UserCode
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
                if (data == null || !data.HasValues)
                {
                    // Log and notify if "data" field is missing or null
                    _logger.LogError("Response contains no 'data' field.");
                    TempData["Message"] = "No 'data' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                JArray insertedbranchcodeS = (JArray)jsonObject["data"]["insertedbranchcodeS"];
                JArray existingbranchcodeS = (JArray)jsonObject["data"]["existingbranchcodeS"];

                // Log the list of Building codes (if any)
                if (insertedbranchcodeS.Any())
                {
                    _logger.LogInformation($"Mapping new branch codes : {string.Join(", ", insertedbranchcodeS)}");
                    TempData["Message"] = $"Registration process complete successfully to the following branch code(s) {string.Join(", ", insertedbranchcodeS)}";
                    TempData["MessageType"] = "Success";
                }
                else
                {
                    _logger.LogWarning("No buildings found in the response.");
                    TempData["Message"] = "No buildings found in the API response.";
                    TempData["MessageType"] = "Warning";
                }

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

            // Retrieve the BuildingName from session
            var buildingName = HttpContext.Session.GetObject<string>("branchname");
            if (buildingName == null)
            {
                return RedirectToPage("/Login/Index");
            }
            TempData["branchname"] = buildingName;

            bool successBldNo = await BranchesCallApiAsync(verifyLoginData.SignonName);
            if (!successBldNo)
            {
                _logger.LogError("Failed to retrieve bulidings.");
                return Page(); // Stay on the same page if API call fails
            }

            bool successUser = await UserListCallApiAsync();
            if (!successUser)
            {
                _logger.LogError("Failed to retrieve Users.");
                return Page(); // Stay on the same page if API call fails
            }

            bool successReport = await ReportCallApiAsync(verifyLoginData.BranchCode);
            if (!successReport)
            {
                _logger.LogError("Failed to retrieve reports.");
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

        public class MappingForm
        {
            public int MAPPINGID { get; set; }
            public int USERCODE { get; set; }
            public string? BRANCHCODE { get; set; }
            public string? PROJECTCODE { get; set; }
            public int? ENTRYUSER { get; set; }
            public DateTime? CREATEDATE { get; set; }
            public string? ENABLEYN { get; set; }

        }

    }
}