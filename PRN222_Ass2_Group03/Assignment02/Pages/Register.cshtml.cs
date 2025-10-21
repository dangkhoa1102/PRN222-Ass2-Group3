using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Assignment02.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IUserService userService, ILogger<RegisterModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 255 characters")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                Response.Redirect("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if username is available
                if (!await _userService.IsUsernameAvailableAsync(Username))
                {
                    ErrorMessage = "Username is already taken. Please choose a different username.";
                    return Page();
                }

                // Check if email is available
                if (!await _userService.IsEmailAvailableAsync(Email))
                {
                    ErrorMessage = "Email is already registered. Please use a different email or try logging in.";
                    return Page();
                }

                // Create new user DTO
                var newUserDto = new UserDTO
                {
                    Username = Username,
                    Email = Email,
                    FullName = FullName,
                    Phone = Phone,
                    Role = "Customer", // Default role
                    IsActive = true
                };

                var createdUser = await _userService.CreateUserAsync(newUserDto, Password);

                if (createdUser != null)
                {
                    _logger.LogInformation("New user registered successfully: {Username}", Username);
                    
                    // Set success message and redirect to Login page
                    TempData["RegistrationSuccessMessage"] = "Registration successful! Please login with your credentials.";
                    return RedirectToPage("/Login");
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during registration. Please try again.";
                _logger.LogError(ex, "Error during user registration for username: {Username}", Username);
            }

            return Page();
        }
    }
}