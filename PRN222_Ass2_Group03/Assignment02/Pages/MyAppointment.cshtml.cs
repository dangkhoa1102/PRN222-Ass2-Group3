using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class MyAppointmentModel : PageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public MyAppointmentModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IEnumerable<TestDriveAppointment> Appointments { get; set; } = new List<TestDriveAppointment>();

        [BindProperty]
        public string CancelNote { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Lấy CustomerId từ session
            var customerIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerIdString) || !Guid.TryParse(customerIdString, out Guid customerId))
            {
                // Nếu không có CustomerId hoặc không hợp lệ, chuyển hướng đến trang đăng nhập
                Response.Redirect("/Login");
                return;
            }

            // Lấy danh sách các cuộc hẹn của khách hàng
            Appointments = await _appointmentService.GetCustomerAppointmentsAsync(customerId);
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            // Lấy UserId từ session
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Gọi hàm CancelAppointmentAsync với note
                var success = await _appointmentService.CancelAppointmentAsync(id, userId, CancelNote);
                if (!success)
                {
                    ModelState.AddModelError("", "Không thể hủy lịch hẹn. Vui lòng kiểm tra lại.");
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
    }
}
