using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages
{
    public class BookTestDriveModel : PageModel
    {
        private readonly ICustomerTestDriveAppointmentService _appointmentService;
        private readonly IVehicleService _vehicleService;
        
        public BookTestDriveModel(ICustomerTestDriveAppointmentService appointmentService, IVehicleService vehicleService)
        {
            _appointmentService = appointmentService;
            _vehicleService = vehicleService;
        }

        [BindProperty]
        public TestDriveAppointmentDTO Appointment { get; set; } = new TestDriveAppointmentDTO();

        [BindProperty]
        public string SelectedTimeSlot { get; set; } = string.Empty;

        public IList<DealerDTO> Dealers { get; set; } = new List<DealerDTO>();
        public IList<VehicleDTO> Vehicles { get; set; } = new List<VehicleDTO>();
        public IList<TestDriveAppointmentDTO> AvailableTimeSlots { get; set; } = new List<TestDriveAppointmentDTO>();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(Guid? vehicleId, Guid? dealerId, DateTime? date)
        {
            Dealers = (await _appointmentService.GetAllDealersAsync()).ToList();
            AvailableTimeSlots = new List<TestDriveAppointmentDTO>();
            
            // Always load all vehicles for selection
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            Vehicles = allVehicles.ToList();
            
            // Pre-select vehicle if vehicleId is provided
            if (vehicleId.HasValue)
            {
                var selectedVehicle = allVehicles.FirstOrDefault(v => v.Id == vehicleId.Value);
                if (selectedVehicle != null)
                {
                    Appointment.VehicleId = selectedVehicle.Id;
                }
            }
            
            // Pre-select dealer if dealerId is provided
            if (dealerId.HasValue)
            {
                Appointment.DealerId = dealerId.Value;
            }
            
            // Pre-select date if date is provided
            if (date.HasValue)
            {
                Appointment.AppointmentDate = date.Value;
            }
            
            // Load time slots if both dealer and date are provided
            if (dealerId.HasValue && date.HasValue)
            {
                try
                {
                    AvailableTimeSlots = (await _appointmentService.GetAvailableTimeSlotsAsync(dealerId.Value, date.Value)).ToList();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Lỗi khi tải khung giờ: {ex.Message}";
                    AvailableTimeSlots = new List<TestDriveAppointmentDTO>();
                }
            }
        }

        public async Task<IActionResult> OnGetVehiclesAsync(Guid dealerId)
        {
            // Tất cả xe đều có thể được lái thử ở bất kỳ đại lý nào
            // Vì Vehicle model không có DealerId
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
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
                if (string.IsNullOrEmpty(SelectedTimeSlot))
                {
                    return new JsonResult(new { success = false, message = "Vui lòng chọn giờ hẹn." });
                }

                var timeParts = SelectedTimeSlot.Split(':');
                if (timeParts.Length != 2)
                {
                    return new JsonResult(new { success = false, message = "Định dạng giờ không hợp lệ." });
                }

                if (!int.TryParse(timeParts[0], out int hour) || !int.TryParse(timeParts[1].Split(' ')[0], out int minute))
                {
                    return new JsonResult(new { success = false, message = "Giờ không hợp lệ." });
                }

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

                if (!Guid.TryParse(customerIdString, out Guid customerId))
                {
                    return new JsonResult(new { success = false, message = "Thông tin người dùng không hợp lệ." });
                }

                // Validate required fields
                if (Appointment.DealerId == Guid.Empty)
                {
                    return new JsonResult(new { success = false, message = "Vui lòng chọn đại lý." });
                }

                if (Appointment.VehicleId == Guid.Empty)
                {
                    return new JsonResult(new { success = false, message = "Vui lòng chọn xe." });
                }

                // Book appointment
                var appointment = await _appointmentService.BookAppointmentAsync(
                    customerId,
                    Appointment.DealerId,
                    Appointment.VehicleId,
                    Appointment.AppointmentDate,
                    Appointment.Notes ?? string.Empty
                );

                // Kiểm tra status của appointment
                bool isSuccess = appointment.Status == "pending";

                return new JsonResult(new
                {
                    success = isSuccess,
                    message = isSuccess ? "Đặt lịch hẹn thành công!" : $"Đặt lịch thất bại. Trạng thái lịch hẹn: {appointment.Status}."
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return new JsonResult(new { success = false, message = "Lỗi cơ sở dữ liệu: Trạng thái lịch hẹn không hợp lệ. Vui lòng thử lại hoặc liên hệ quản trị viên." });
            }
            catch (InvalidOperationException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "Đã xảy ra lỗi khi đặt lịch. Vui lòng thử lại." });
            }
        }
    }
}

