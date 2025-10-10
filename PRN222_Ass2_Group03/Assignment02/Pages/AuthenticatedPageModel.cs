using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class AuthenticatedPageModel : PageModel
    {
        public string? UserId { get; private set; }
        public string? CurrentUsername { get; private set; }
        public string? CurrentUserFullName { get; private set; }
        public string? CurrentUserRole { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            // Check authentication
            UserId = HttpContext.Session.GetString("UserId");
            CurrentUsername = HttpContext.Session.GetString("Username");
            CurrentUserFullName = HttpContext.Session.GetString("FullName");
            CurrentUserRole = HttpContext.Session.GetString("Role");
            IsAuthenticated = !string.IsNullOrEmpty(UserId);

            // Redirect to login if not authenticated
            if (!IsAuthenticated)
            {
                context.Result = RedirectToPage("/Login");
                return;
            }

            base.OnPageHandlerExecuting(context);
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}