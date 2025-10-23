using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Assignment02.Services;

namespace Assignment02.Pages.TestDrives
{
    public class DetailsModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;
        private readonly RealTimeNotificationService _notificationService;

        public DetailsModel(ICustomerTestDriveAppointmentService appointmentService, RealTimeNotificationService notificationService)
        {
            _appointmentService = appointmentService;
            _notificationService = notificationService;
        }

        public TestDriveAppointmentDTO? Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            if (!Guid.TryParse(UserId, out var userId))
            {
                return RedirectToPage("/Login");
            }

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Owner check
            if (appointment.CustomerId != userId)
            {
                return Forbid();
            }

            Appointment = appointment;
            return Page();
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
                return Forbid();
            }
            
            // Gửi SignalR notification
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment != null)
            {
                await _notificationService.NotifyTestDriveUpdated(
                    appointment.Customer?.FullName ?? "Unknown Customer",
                    appointment.Vehicle?.Name ?? "Unknown Vehicle",
                    "Completed"
                );
                await _notificationService.NotifyPageReload("appointments", "completed");
            }
            
            return RedirectToPage(new { id });
        }
    }
}


