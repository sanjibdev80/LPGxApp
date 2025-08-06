using LPGxWebApp.Class;
using LPGxWebApp.GlobalData;
using LPGxWebApp.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LPGxWebApp.Pages
{
    public class ProjectsModel : PageModel
    {
        private readonly ILogger<ProjectsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        // Add a property to hold the project data
        public List<ProjectResponse> Projects { get; set; }

        [BindProperty]
        public ProjectForm projectForm { get; set; }

        public ProjectsModel(ILogger<ProjectsModel> logger, IConfiguration configuration, ApiService apiService)
        {
            _logger = logger;
            _configuration = configuration;
            _apiService = apiService;
        }
        // Consolidated method to handle API calls
        private async Task<bool> CallApiAsync()
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
                string apiUrl = $"{baseUrl}ProjectInfos";

                // Call the API using ApiService's CallApiAsync method
                var response = await _apiService.CallApiAsync("GET", apiUrl, string.Empty);
                if (string.IsNullOrEmpty(response) || response.Contains("No record found"))
                {
                    _logger.LogWarning($"No response from API");
                    return false;
                }

                // Deserialize the response to ProjectResponse
                var projectResponse = JsonConvert.DeserializeObject<List<ProjectResponse>>(response);


                if (projectResponse == null)
                {
                    _logger.LogError($"Failed to deserialize response");
                    return false;
                }

                // Assign the data to the Projects property
                Projects = projectResponse;

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
                string apiUrl = $"{baseUrl}ProjectInfos";

                // Prepare the request body
                var requestBody = new
                {
                    projectcode = projectForm.PROJECTCODE,
                    countrycode = projectForm.COUNTRYCODE,
                    projectname = projectForm.PROJECTNAME?.ToUpper(),
                    projectaddress = projectForm.PROJECTADDRESS?.ToUpper(),
                    entryuser = projectForm.ENTRYUSER,
                    createdate = projectForm.CREATEDATE,
                    enableyn = projectForm.ENABLEYN
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
                var projectcode = data["projectcode"]?.ToString();
                if (string.IsNullOrEmpty(projectcode))
                {
                    // Log and notify if "projectcode" is missing or empty
                    _logger.LogError("Response contains no 'projectcode' field.");
                    TempData["Message"] = "No 'projectcode' field in the API response.";
                    TempData["MessageType"] = "Error";
                    return false;
                }

                // Log the successful response with the Building code
                _logger.LogInformation($"Data saved successfully. New project code is: {projectcode}");

                // Set a success message for the user
                TempData["Message"] = $"Data saved successfully. New project code is: {projectcode}";
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

            // Retrieve the BuildingName from session
            var buildingName = HttpContext.Session.GetObject<string>("branchname");
            if (buildingName == null)
            {
                return RedirectToPage("/Login/Index");
            }
            TempData["branchname"] = buildingName;

            bool success = await CallApiAsync();
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

        public class ProjectForm
        {
            public string PROJECTCODE { get; set; }
            public string COUNTRYCODE { get; set; }
            public string PROJECTNAME { get; set; }
            public string? PROJECTADDRESS { get; set; }
            public int? ENTRYUSER { get; set; }
            public DateTime? CREATEDATE { get; set; }
            public string? ENABLEYN { get; set; }
        }

    }
}