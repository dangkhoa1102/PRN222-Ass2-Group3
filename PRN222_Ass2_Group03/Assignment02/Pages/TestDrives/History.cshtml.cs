using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.TestDrives
{
    public class HistoryModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public HistoryModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public List<TestDriveAppointment> History { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            if (!Guid.TryParse(UserId, out var userId))
            {
                return RedirectToPage("/Login");
            }

            var all = await _appointmentService.GetCustomerAppointmentsAsync(userId);
            History = all
                .Where(a => a.Status != null && 
                           (a.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) ||
                            a.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return Page();
        }
    }
}


