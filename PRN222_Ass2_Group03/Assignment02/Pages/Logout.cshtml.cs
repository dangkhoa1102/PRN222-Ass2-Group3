using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return OnPost();
        }

        public IActionResult OnPost()
        {
            var username = HttpContext.Session.GetString("Username");
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            if (!string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("User {Username} logged out successfully", username);
            }
            
            // Redirect to login page
            return RedirectToPage("/Login");
        }
    }
}