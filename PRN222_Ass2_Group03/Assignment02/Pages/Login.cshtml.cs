using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IUserService userService, ILogger<LoginModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                Response.Redirect("/Index");
            }

            // Get success message from TempData (from registration)
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please provide valid credentials.";
                return Page();
            }

            try
            {
                var user = await _userService.LoginAsync(Username, Password);
                
                if (user != null)
                {
                    // Store user information in session
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);
                    HttpContext.Session.SetString("Role", user.Role ?? "Customer");
                    
                    _logger.LogInformation("User {Username} logged in successfully", Username);
                    
                    // Redirect to homepage
                    return RedirectToPage("/Index");
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                    _logger.LogWarning("Failed login attempt for username: {Username}", Username);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                _logger.LogError(ex, "Error during login for username: {Username}", Username);
            }

            return Page();
        }
    }
}