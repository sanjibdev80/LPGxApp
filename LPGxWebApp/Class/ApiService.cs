using LPGxWebApp.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace LPGxWebApp.Class
{
    public class ApiService
    {
        private readonly ApiSettings _apiSettings;
        private readonly HttpClient _httpClient;

        // Inject ApiSettings and HttpClient via constructor
        public ApiService(IOptions<ApiSettings> apiSettings, HttpClient httpClient)
        {
            _apiSettings = apiSettings.Value;  // Accessing ApiSettings from IConfiguration
            _httpClient = httpClient;          // Using HttpClient to make API requests
        }

        // Method to call the API
        public async Task<string> CallApiAsync(string requestMethod, string requestUrl, object requestBody)
        {
            try
            {
                // Ensure the request URL is provided
                if (string.IsNullOrEmpty(requestUrl))
                {
                    return "Request URL is missing.";
                }

                // Prepare the HttpRequestMessage
                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(requestUrl),  // Use the provided URL
                    Method = new HttpMethod(requestMethod)  // Set the method (GET, POST, PUT, DELETE, etc.)
                };

                // Add the request body if the method requires it (e.g., POST, PUT)
                if (requestMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) || requestMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    // Serialize the body object to JSON if it's not null
                    if (requestBody != null)
                    {
                        var jsonBody = JsonConvert.SerializeObject(requestBody);
                        requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                    }
                }

                // Send the request
                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

                // Ensure the response is successful (HTTP status code 2xx)
                if (response.IsSuccessStatusCode)
                {
                    // Read the response body as a string
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Optionally, check if the response body is empty
                    if (string.IsNullOrEmpty(responseBody))
                    {
                        // Log and return a message indicating an empty response
                        return "Empty response received.";
                    }

                    // Return the successful response content
                    return responseBody;
                }
                else
                {
                    // Check if the response content type is JSON
                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (contentType != null && contentType.Contains("application/json"))
                    {
                        // Read the response body as a string
                        var responseBody = await response.Content.ReadAsStringAsync();

                        // Optionally, check if the response body is empty
                        if (string.IsNullOrEmpty(responseBody))
                        {
                            // Log and return a message indicating an empty response
                            return "Empty response received.";
                        }

                        // Try to parse the response as JSON
                        JObject jsonResponse = JObject.Parse(responseBody);

                        // Extract the message from the JSON response
                        var message = jsonResponse["message"]?.ToString();

                        // Handle specific error statuses (e.g., 400 Bad Request, 404 Not Found)
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.BadRequest:
                                // Log and handle BadRequest (400) error
                                return $"{message}";
                                //return "The request was invalid. Please check the input parameters and try again.";

                            case HttpStatusCode.NotFound:
                                // Log and handle NotFound (404) error
                                return $"{message}";
                                //return "The requested resource could not be found. Please check the endpoint or resource ID.";

                            // Add more custom cases for other status codes as needed
                            case HttpStatusCode.Unauthorized:
                                return $"{message}";
                                //return "You are not authorized to access this resource. Please check your credentials.";

                            case HttpStatusCode.InternalServerError:
                                return $"{message}";
                                //return "The server encountered an internal error. Please try again later.";

                            // Handle other status codes
                            default:
                                // Log the error for unknown status codes
                                return $"API request failed with status: {response.StatusCode}. Reason: {response.ReasonPhrase}";
                                //return "An unexpected error occurred. Please try again later.";
                        }
                    }
                    else
                    {
                        return $"API request failed";
                    }
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network-related or HTTP-specific errors
                return $"Request error: {e.Message}";
            }
            catch (Exception e)
            {
                // Handle any other types of errors
                return $"Error: {e.Message}";
            }
        }


    }

}




