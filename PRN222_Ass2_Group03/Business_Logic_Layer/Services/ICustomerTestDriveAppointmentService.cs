using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface ICustomerTestDriveAppointmentService
    {
        Task<IEnumerable<TestDriveAppointmentDTO>> GetCustomerAppointmentsAsync(Guid customerId);
        Task<TestDriveAppointmentDTO> BookAppointmentAsync(Guid customerId, Guid dealerId, Guid vehicleId, DateTime appointmentDate, string notes);
        Task<TestDriveAppointmentDTO?> GetAppointmentByIdAsync(Guid id);
        Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid customerId, string note);
        Task<bool> CancelAppointmentByStaffAsync(Guid appointmentId, string note);
        Task<IEnumerable<TestDriveAppointmentDTO>> GetAvailableTimeSlotsAsync(Guid dealerId, DateTime date);
        Task<IEnumerable<VehicleDTO>> GetAvailableVehiclesAsync(Guid dealerId);
        Task<bool> RescheduleAppointmentAsync(Guid appointmentId, DateTime newDateTime, Guid customerId);
        Task<IEnumerable<DealerDTO>> GetAllDealersAsync();
        Task<IEnumerable<TestDriveAppointmentDTO>> GetAllAppointmentsAsync(Guid userId);
        Task<IEnumerable<TestDriveAppointmentDTO>> GetAppointmentsByStatusAsync(string status);
        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status);
        Task<bool> CompleteAppointmentAsync(Guid appointmentId);
        Task<bool> MarkDoneByCustomerAsync(Guid appointmentId, Guid customerId);
    }
}
