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
                    (a.Status != null && a.Status.ToLower() != "cancelled" && a.Status.ToLower() != "done")
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

            // Kiểm tra phân quyền: Chỉ cho phép cancel appointment của chính mình (trừ admin/staff)
            if (string.Equals(UserRole, "Customer", StringComparison.OrdinalIgnoreCase))
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
                // Gọi hàm CancelAppointmentAsync với note từ form
                var success = await _appointmentService.CancelAppointmentAsync(id, userId, CancelNote);
                if (!success)
                {
                    ModelState.AddModelError("", "Không thể hủy lịch hẹn. Vui lòng kiểm tra lại.");
                }
                else
                {
                    // Gửi SignalR notification
                    var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                    if (appointment != null)
                    {
                        await _notificationService.NotifyTestDriveCancelled(
                            appointment.Customer?.FullName ?? "Unknown Customer",
                            appointment.Vehicle?.Name ?? "Unknown Vehicle"
                        );
                        await _notificationService.NotifyPageReload("appointments", "cancelled_by_customer");
                    }
                    
                    TempData["SuccessMessage"] = "Lịch hẹn đã được hủy thành công.";
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
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
                TempData["ErrorMessage"] = "Không thể đánh dấu hoàn thành.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã đánh dấu hoàn thành!";
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
