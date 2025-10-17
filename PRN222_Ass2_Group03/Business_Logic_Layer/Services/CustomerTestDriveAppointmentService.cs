using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public class CustomerTestDriveAppointmentService : ICustomerTestDriveAppointmentService
    {
        private readonly ICustomerTestDriveAppointment _repository;

        public CustomerTestDriveAppointmentService(ICustomerTestDriveAppointment repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetCustomerAppointmentsAsync(Guid customerId)
        {
            return await _repository.GetByCustomerIdAsync(customerId);
        }

        public async Task<TestDriveAppointment> BookAppointmentAsync(Guid customerId, Guid dealerId, Guid vehicleId, DateTime appointmentDate, string notes)
        {
            // Validate time slot availability
            var timeSpan = appointmentDate.TimeOfDay;
            var isAvailable = await _repository.IsTimeSlotAvailableAsync(dealerId, appointmentDate, timeSpan);

            if (!isAvailable)
            {
                throw new InvalidOperationException("Thời gian này đã được đặt. Vui lòng chọn thời gian khác.");
            }

            var appointment = new TestDriveAppointment
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                DealerId = dealerId,
                VehicleId = vehicleId,
                AppointmentDate = appointmentDate,
                Status = "pending",
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateAsync(appointment);
        }

        public async Task<TestDriveAppointment> GetAppointmentByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid customerId, string note)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false;
            }

            // Only allow cancellation if appointment is more than 24 hours away
            if (appointment.AppointmentDate <= DateTime.Now.AddHours(24))
            {
                throw new InvalidOperationException("Không thể hủy lịch hẹn trong vòng 24 giờ trước giờ hẹn.");
            }

            appointment.Status = "Cancelled";
            appointment.Notes = note;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetAvailableTimeSlotsAsync(Guid dealerId, DateTime date)
        {
            return await _repository.GetAvailableAppointmentsAsync(dealerId, date);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(Guid dealerId)
        {
            return await _repository.GetAvailableVehiclesAsync(dealerId);
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _repository.GetAllDealersAsync();
        }

        public async Task<bool> RescheduleAppointmentAsync(Guid appointmentId, DateTime newDateTime, Guid customerId)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false;
            }

            // Check if new time slot is available
            var isAvailable = await _repository.IsTimeSlotAvailableAsync(appointment.DealerId, newDateTime, newDateTime.TimeOfDay);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Thời gian mới không khả dụng.");
            }

            appointment.AppointmentDate = newDateTime;
            appointment.Status = "Rescheduled";
            appointment.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<IEnumerable<TestDriveAppointment>> GetAllAppointmentsAsync(Guid userId)
        {
            return await _repository.GetAllAppointmentsAsync(userId);
        }
    }
}
