using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public class CustomerTestDriveAppointmentService : ICustomerTestDriveAppointmentService
    {
        private readonly ICustomerTestDriveAppointment _repository;

        public CustomerTestDriveAppointmentService(ICustomerTestDriveAppointment repository)
        {
            _repository = repository;
        }

        private TestDriveAppointmentDTO ConvertToDTO(TestDriveAppointment appointment)
        {
            return new TestDriveAppointmentDTO
            {
                Id = appointment.Id,
                CustomerId = appointment.CustomerId,
                DealerId = appointment.DealerId,
                VehicleId = appointment.VehicleId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                Customer = appointment.Customer != null ? new UserDTO
                {
                    Id = appointment.Customer.Id,
                    Username = appointment.Customer.Username,
                    Email = appointment.Customer.Email,
                    FullName = appointment.Customer.FullName,
                    Phone = appointment.Customer.Phone,
                    // Address = appointment.Customer.Address, // User model không có Address property
                    Role = appointment.Customer.Role,
                    IsActive = appointment.Customer.IsActive,
                    CreatedAt = appointment.Customer.CreatedAt
                } : null,
                Dealer = appointment.Dealer != null ? new DealerDTO
                {
                    Id = appointment.Dealer.Id,
                    Name = appointment.Dealer.Name,
                    Address = appointment.Dealer.Address,
                    Phone = appointment.Dealer.Phone,
                    Email = appointment.Dealer.Email,
                    IsActive = appointment.Dealer.IsActive,
                    CreatedAt = appointment.Dealer.CreatedAt
                } : null,
                Vehicle = appointment.Vehicle != null ? new VehicleDTO
                {
                    Id = appointment.Vehicle.Id,
                    Name = appointment.Vehicle.Name,
                    Brand = appointment.Vehicle.Brand,
                    Model = appointment.Vehicle.Model,
                    Year = appointment.Vehicle.Year ?? 0,
                    Price = appointment.Vehicle.Price,
                    Description = appointment.Vehicle.Description,
                    Specifications = appointment.Vehicle.Specifications,
                    Images = appointment.Vehicle.Images,
                    StockQuantity = appointment.Vehicle.StockQuantity ?? 0,
                    IsActive = appointment.Vehicle.IsActive,
                    CreatedAt = appointment.Vehicle.CreatedAt
                } : null
            };
        }

        public async Task<IEnumerable<TestDriveAppointmentDTO>> GetCustomerAppointmentsAsync(Guid customerId)
        {
            var appointments = await _repository.GetByCustomerIdAsync(customerId);
            return appointments.Select(ConvertToDTO);
        }

        public async Task<TestDriveAppointmentDTO> BookAppointmentAsync(Guid customerId, Guid dealerId, Guid vehicleId, DateTime appointmentDate, string notes)
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
                CreatedAt = DateTime.Now
            };

            var createdAppointment = await _repository.CreateAsync(appointment);
            return ConvertToDTO(createdAppointment);
        }

        public async Task<TestDriveAppointmentDTO?> GetAppointmentByIdAsync(Guid id)
        {
            var appointment = await _repository.GetByIdAsync(id);
            return appointment != null ? ConvertToDTO(appointment) : null;
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid customerId, string note)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false;
            }

            // Allow cancellation only when appointment is in processing/pending status
            var status = appointment.Status?.ToLower();
            if (status != "pending" && status != "processing")
            {
                throw new InvalidOperationException("Chỉ có thể hủy lịch hẹn khi đang ở trạng thái Processing.");
            }

            appointment.Status = "Cancelled";
            appointment.Notes = note;
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> CancelAppointmentByStaffAsync(Guid appointmentId, string note)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = "Cancelled";
            appointment.Notes = note;
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<IEnumerable<TestDriveAppointmentDTO>> GetAvailableTimeSlotsAsync(Guid dealerId, DateTime date)
        {
            var appointments = await _repository.GetAvailableAppointmentsAsync(dealerId, date);
            return appointments.Select(ConvertToDTO);
        }

        public async Task<IEnumerable<VehicleDTO>> GetAvailableVehiclesAsync(Guid dealerId)
        {
            var vehicles = await _repository.GetAvailableVehiclesAsync(dealerId);
            return vehicles.Select(v => new VehicleDTO
            {
                Id = v.Id,
                Name = v.Name,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year ?? 0,
                Price = v.Price,
                Description = v.Description,
                Specifications = v.Specifications,
                Images = v.Images,
                StockQuantity = v.StockQuantity ?? 0,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt
            });
        }

        public async Task<IEnumerable<DealerDTO>> GetAllDealersAsync()
        {
            var dealers = await _repository.GetAllDealersAsync();
            return dealers.Select(d => new DealerDTO
            {
                Id = d.Id,
                Name = d.Name,
                Address = d.Address,
                Phone = d.Phone,
                Email = d.Email,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            });
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
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<IEnumerable<TestDriveAppointmentDTO>> GetAllAppointmentsAsync(Guid userId)
        {
            var appointments = await _repository.GetAllAppointmentsAsync(userId);
            return appointments.Select(ConvertToDTO);
        }

        public async Task<IEnumerable<TestDriveAppointmentDTO>> GetAppointmentsByStatusAsync(string status)
        {
            // This would need to be implemented in the repository
            // For now, we'll get all appointments and filter by status
            var allAppointments = await _repository.GetAllAppointmentsAsync(Guid.Empty);
            return allAppointments.Where(a => a.Status?.ToLower() == status.ToLower()).Select(ConvertToDTO);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }
            appointment.Status = "Completed";
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> MarkDoneByCustomerAsync(Guid appointmentId, Guid customerId)
        {
            var appointment = await _repository.GetByIdAsync(appointmentId);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                return false;
            }
            if (!string.Equals(appointment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            appointment.Status = "Done";
            appointment.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(appointment);
            return true;
        }
    }
}
