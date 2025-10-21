using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class NotificationCenterModel : PageModel
    {
        public string? CurrentUserId { get; set; }
        public string? CurrentUserRole { get; set; }

        public void OnGet()
        {
            CurrentUserId = HttpContext.Session.GetString("UserId");
            CurrentUserRole = HttpContext.Session.GetString("UserRole");
        }
    }
}
