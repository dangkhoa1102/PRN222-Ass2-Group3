using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public async Task<IActionResult> OnGetAsync()
        {
            return await OnPostAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = HttpContext.Session.GetString("Username");
            
            // Sign out from authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
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