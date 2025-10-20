using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Admin
{
    public class ManageAppointmentsModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public ManageAppointmentsModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IEnumerable<TestDriveAppointment> Appointments { get; set; } = new List<TestDriveAppointment>();
        public string? FilterStatus { get; set; }

        [BindProperty]
        public string CancelNote { get; set; } = string.Empty;

        public string UserRole => CurrentUserRole ?? "Customer";

        public async Task OnGetAsync(string? status = null)
        {
            // Kiểm tra quyền Admin hoặc Staff
            if (UserRole != "Admin" && UserRole != "Staff")
            {
                Response.Redirect("/MyAppointment");
                return;
            }

            FilterStatus = status;
            
            // Lấy tất cả appointments hoặc filter theo status
            if (string.IsNullOrEmpty(status) || status == "all")
            {
                Appointments = await _appointmentService.GetAllAppointmentsAsync(Guid.Empty);
            }
            else
            {
                Appointments = await _appointmentService.GetAppointmentsByStatusAsync(status);
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
        {
            // Kiểm tra quyền Admin hoặc Staff
            if (UserRole != "Admin" && UserRole != "Staff")
            {
                return Forbid();
            }

            try
            {
                var success = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Appointment status updated to {status} successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update appointment status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating appointment: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            // Kiểm tra quyền Admin hoặc Staff
            if (UserRole != "Admin" && UserRole != "Staff")
            {
                return Forbid();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var success = await _appointmentService.CancelAppointmentAsync(id, userId, CancelNote);
                if (success)
                {
                    TempData["SuccessMessage"] = "Appointment cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel appointment.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling appointment: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Helper method để kiểm tra quyền cancel
        public bool CanCancelAppointment(TestDriveAppointment appointment)
        {
            if (appointment.Status == "Cancelled")
                return false;

            // Admin và Staff có thể cancel bất kỳ appointment nào
            return UserRole == "Admin" || UserRole == "Staff";
        }
    }
}
