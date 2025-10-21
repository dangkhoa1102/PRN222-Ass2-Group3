using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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

            // Only show registration success message, not order-related messages
            if (TempData["RegistrationSuccessMessage"] != null)
            {
                SuccessMessage = TempData["RegistrationSuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

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
                    // Tạo claims để hỗ trợ [Authorize]
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role ?? "Customer"),
                        new Claim("FullName", user.FullName ?? user.Username)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in the user with authentication
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Store user information in session (giữ nguyên logic hiện tại)
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FullName", user.FullName ?? user.Username ?? string.Empty);
                    HttpContext.Session.SetString("Role", user.Role ?? "Customer");

                    _logger.LogInformation("User {Username} logged in successfully", Username);

                    // Redirect to returnUrl or homepage
                    return LocalRedirect(returnUrl);
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