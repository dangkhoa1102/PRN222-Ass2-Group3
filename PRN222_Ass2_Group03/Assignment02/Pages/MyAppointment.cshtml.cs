using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class MyAppointmentModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public MyAppointmentModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IEnumerable<TestDriveAppointment> Appointments { get; set; } = new List<TestDriveAppointment>();

        [BindProperty]
        public string CancelNote { get; set; } = string.Empty;

        public string UserRole => CurrentUserRole ?? "Customer";

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
            Appointments = await _appointmentService.GetCustomerAppointmentsAsync(customerId);
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

            // Kiểm tra phân quyền: Chỉ cho phép cancel appointment của chính mình (trừ admin/staff)
            if (UserRole == "Customer")
            {
                // Kiểm tra xem appointment có thuộc về user này không
                var appointment = Appointments.FirstOrDefault(a => a.Id == id);
                if (appointment == null || appointment.CustomerId != userId)
                {
                    ModelState.AddModelError("", "Bạn chỉ có thể hủy lịch hẹn của chính mình.");
                    Appointments = await _appointmentService.GetCustomerAppointmentsAsync(userId);
                    return Page();
                }
            }

            try
            {
                // Gọi hàm CancelAppointmentAsync với note
                var success = await _appointmentService.CancelAppointmentAsync(id, userId, CancelNote);
                if (!success)
                {
                    ModelState.AddModelError("", "Không thể hủy lịch hẹn. Vui lòng kiểm tra lại.");
                }
                else
                {
                    TempData["SuccessMessage"] = "Lịch hẹn đã được hủy thành công.";
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // Làm mới danh sách lịch hẹn
            Appointments = await _appointmentService.GetCustomerAppointmentsAsync(userId);
            return Page();
        }

        // Helper method để kiểm tra quyền cancel
        public bool CanCancelAppointment(TestDriveAppointment appointment)
        {
            if (appointment.Status == "Cancelled" || appointment.AppointmentDate <= DateTime.Now.AddHours(24))
                return false;

            // Admin và Staff có thể cancel bất kỳ appointment nào
            if (UserRole == "Admin" || UserRole == "Staff")
                return true;

            // Customer chỉ có thể cancel appointment của mình
            if (UserRole == "Customer")
                return appointment.CustomerId.ToString() == UserId;

            return false;
        }
    }
}
