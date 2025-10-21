using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class BookTestDriveModel : PageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;
        public BookTestDriveModel(ICustomerTestDriveAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [BindProperty]
        public TestDriveAppointment Appointment { get; set; }

        [BindProperty]
        public string SelectedTimeSlot { get; set; }

        public IList<Dealer> Dealers { get; set; }
        public IList<Vehicle> Vehicles { get; set; }
        public IList<TestDriveAppointment> AvailableTimeSlots { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Dealers = (await _appointmentService.GetAllDealersAsync()).ToList();
            Vehicles = new List<Vehicle>();
            AvailableTimeSlots = new List<TestDriveAppointment>();
        }

        public async Task<IActionResult> OnGetVehiclesAsync(Guid dealerId)
        {
            var vehicles = await _appointmentService.GetAvailableVehiclesAsync(dealerId);
            Console.WriteLine(vehicles);
            return new JsonResult(vehicles);
        }

        public async Task<IActionResult> OnGetTimeSlotsAsync(Guid dealerId, DateTime date)
        {
            var timeSlots = await _appointmentService.GetAvailableTimeSlotsAsync(dealerId, date);
            return new JsonResult(timeSlots);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
            }

            try
            {
                // Parse selected time slot and combine with date
                var timeParts = SelectedTimeSlot.Split(':');
                var hour = int.Parse(timeParts[0]);
                var minute = int.Parse(timeParts[1].Split(' ')[0]);
                var isPM = timeParts[1].Contains("PM");
                if (isPM && hour != 12) hour += 12;
                if (!isPM && hour == 12) hour = 0;

                Appointment.AppointmentDate = Appointment.AppointmentDate.Date.Add(new TimeSpan(hour, minute, 0));

                // Get CustomerId from session
                var customerIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(customerIdString))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để đặt lịch hẹn." });
                }

                var customerId = Guid.Parse(customerIdString);

                // Book appointment
                var appointment = await _appointmentService.BookAppointmentAsync(
                    customerId,
                    Appointment.DealerId,
                    Appointment.VehicleId,
                    Appointment.AppointmentDate,
                    Appointment.Notes
                );

                // Kiểm tra status của appointment
                bool isSuccess = appointment.Status == "pending";

                return new JsonResult(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Đặt lịch hẹn thành công!" : $"Đặt lịch thất bại. Trạng thái lịch hẹn: {appointment.Status}."
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                return new JsonResult(new { success = false, message = "Lỗi cơ sở dữ liệu: Trạng thái lịch hẹn không hợp lệ. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
            catch (InvalidOperationException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Đã xảy ra lỗi khi đặt lịch. Vui lòng thử lại." });
            }
        }
    }
}

