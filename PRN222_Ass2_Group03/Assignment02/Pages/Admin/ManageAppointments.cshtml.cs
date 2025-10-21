using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Assignment02.Pages.Admin
{
    public class ManageAppointmentsModel : AuthenticatedPageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;

        public ManageAppointmentsModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IEnumerable<TestDriveAppointmentDTO> Appointments { get; set; } = new List<TestDriveAppointmentDTO>();
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

            // Bỏ filter: luôn tải tất cả lịch hẹn, sắp xếp mới nhất trước
            FilterStatus = null;
            var list = await _appointmentService.GetAllAppointmentsAsync(Guid.Empty);
            Appointments = list.OrderByDescending(a => a.AppointmentDate).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string handler, Guid id, string status)
        {
            // Kiểm tra quyền Admin hoặc Staff
            if (UserRole != "Admin" && UserRole != "Staff")
            {
                return Forbid();
            }

            if (handler == "UpdateStatus")
            {
                return await HandleUpdateStatus(id, status);
            }
            else if (handler == "Cancel")
            {
                return await HandleCancel(id);
            }

            return RedirectToPage();
        }

        private async Task<IActionResult> HandleUpdateStatus(Guid id, string status)
        {
            try
            {
                bool success = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Cập nhật trạng thái thành {status} thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
        {
            return await HandleUpdateStatus(id, status);
        }

        private async Task<IActionResult> HandleCancel(Guid id)
        {
            try
            {
                var success = await _appointmentService.CancelAppointmentByStaffAsync(id, CancelNote);
                if (success)
                {
                    TempData["SuccessMessage"] = "Hủy lịch hẹn thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy lịch hẹn.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            return await HandleCancel(id);
        }

        // Helper method để kiểm tra quyền cancel
        public bool CanCancelAppointment(TestDriveAppointmentDTO appointment)
        {
            if (appointment.Status == "Cancelled")
                return false;

            // Admin và Staff có thể cancel bất kỳ appointment nào
            return string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(UserRole, "Staff", StringComparison.OrdinalIgnoreCase);
        }
    }
}
