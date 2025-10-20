using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public interface ICustomerTestDriveAppointmentService
    {
        Task<IEnumerable<TestDriveAppointment>> GetCustomerAppointmentsAsync(Guid customerId);
        Task<TestDriveAppointment> BookAppointmentAsync(Guid customerId, Guid dealerId, Guid vehicleId, DateTime appointmentDate, string notes);
        Task<TestDriveAppointment?> GetAppointmentByIdAsync(Guid id);
        Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid customerId, string note);
        Task<bool> CancelAppointmentByStaffAsync(Guid appointmentId, string note);
        Task<IEnumerable<TestDriveAppointment>> GetAvailableTimeSlotsAsync(Guid dealerId, DateTime date);
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(Guid dealerId);
        Task<bool> RescheduleAppointmentAsync(Guid appointmentId, DateTime newDateTime, Guid customerId);
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<IEnumerable<TestDriveAppointment>> GetAllAppointmentsAsync(Guid userId);
        Task<IEnumerable<TestDriveAppointment>> GetAppointmentsByStatusAsync(string status);
        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status);
        Task<bool> CompleteAppointmentAsync(Guid appointmentId);
        Task<bool> MarkDoneByCustomerAsync(Guid appointmentId, Guid customerId);
    }
}
