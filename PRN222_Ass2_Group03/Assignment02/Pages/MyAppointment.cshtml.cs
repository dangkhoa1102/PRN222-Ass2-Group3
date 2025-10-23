using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Assignment02.Services;

namespace Assignment02.Pages
{
    public class MyAppointmentModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;
        private readonly RealTimeNotificationService _notificationService;

        public MyAppointmentModel(ICustomerTestDriveAppointmentService appointmentService, RealTimeNotificationService notificationService)
        {
            _appointmentService = appointmentService;
            _notificationService = notificationService;
        }

        public IEnumerable<TestDriveAppointmentDTO> Appointments { get; set; } = new List<TestDriveAppointmentDTO>();

        [BindProperty]
        public string CancelNote { get; set; } = string.Empty;

        public string UserRole => CurrentUserRole ?? "Customer";
        public string CurrentUserId => UserId ?? "";

        public async Task OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                Response.Redirect("/Login");
                return;
            }

            // Lấy CustomerId từ session
            var customerIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                Response.Redirect("/Login");
                return;
            }

            // Lấy danh sách các cuộc hẹn của khách hàng
            var allAppointments = await _appointmentService.GetCustomerAppointmentsAsync(customerId);

            // Hiển thị các lịch hiện tại/đang xử lý + Completed (để user mark Done)
            Appointments = allAppointments
                .Where(a =>
                    (a.Status != null && 
                     a.Status.ToLower() != "cancelled" && 
                     a.Status.ToLower() != "done")
                    && a.AppointmentDate >= DateTime.Now.AddDays(-1))
                .OrderBy(a => a.AppointmentDate)
                .ToList();
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            // Lấy UserId từ session
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Gọi hàm CancelAppointmentAsync với note từ form
                var success = await _appointmentService.CancelAppointmentAsync(id, userId, CancelNote);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Unable to cancel the appointment. Please try again.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Appointment has been cancelled successfully.";
                    
                    // Send SignalR notification
                    var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                    if (appointment != null)
                    {
                        Console.WriteLine($"Sending SignalR notification for cancelled appointment: {appointment.Id}");
                        await _notificationService.NotifyTestDriveCancelled(
                            appointment.Customer?.FullName ?? "Unknown Customer",
                            appointment.Vehicle?.Name ?? "Unknown Vehicle"
                        );
                        await _notificationService.NotifyPageReload("appointments", "cancelled");
                        Console.WriteLine("SignalR notifications sent successfully");
                    }
                    else
                    {
                        Console.WriteLine("Appointment not found for SignalR notification");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling the appointment.";
                Console.WriteLine($"Error cancelling appointment: {ex.Message}");
            }

            // Làm mới danh sách và quay lại trang với thông báo
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkDoneAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }
            if (!Guid.TryParse(UserId, out var userId))
            {
                return RedirectToPage("/Login");
            }

            var success = await _appointmentService.MarkDoneByCustomerAsync(id, userId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Unable to mark as done.";
            }
            else
            {
                TempData["SuccessMessage"] = "Appointment marked as done successfully!";
                
                // Send SignalR notification
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment != null)
                {
                    await _notificationService.NotifyTestDriveUpdated(
                        appointment.Customer?.FullName ?? "Unknown Customer",
                        appointment.Vehicle?.Name ?? "Unknown Vehicle",
                        "Done"
                    );
                    await _notificationService.NotifyPageReload("appointments", "done");
                }
            }
            return RedirectToPage();
        }

        // Helper method để kiểm tra quyền cancel
        public bool CanCancelAppointment(TestDriveAppointmentDTO appointment)
        {
            if (appointment.Status == "Cancelled" || appointment.AppointmentDate <= DateTime.Now.AddHours(24))
                return false;

            // Admin và Staff có thể cancel bất kỳ appointment nào
            if (string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(UserRole, "Staff", StringComparison.OrdinalIgnoreCase))
                return true;

            // Customer chỉ có thể cancel appointment của mình
            if (string.Equals(UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
                return appointment.CustomerId.ToString() == UserId;

            return false;
        }
    }
}
