using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LPGxWebApp.Pages
{
    public class MaintenanceModel : PageModel
    {
        private readonly ILogger<MaintenanceModel> _logger;

        public MaintenanceModel(ILogger<MaintenanceModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}