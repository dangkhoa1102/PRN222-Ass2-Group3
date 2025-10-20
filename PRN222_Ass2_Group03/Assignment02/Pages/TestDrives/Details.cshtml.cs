using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.TestDrives
{
    public class DetailsModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public DetailsModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public TestDriveAppointment? Appointment { get; set; }

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
            return RedirectToPage(new { id });
        }
    }
}


