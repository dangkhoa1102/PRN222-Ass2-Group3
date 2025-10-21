using EVDealerDbContext.Models;


namespace DataAccess_Layer.Repositories
{
    public interface ICustomerTestDriveAppointment
    {
        Task<TestDriveAppointment?> GetByIdAsync(Guid id);
        Task<IEnumerable<TestDriveAppointment>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<TestDriveAppointment>> GetAvailableAppointmentsAsync(Guid dealerId, DateTime date);
        Task<TestDriveAppointment> CreateAsync(TestDriveAppointment appointment);
        Task<TestDriveAppointment> UpdateAsync(TestDriveAppointment appointment);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> IsTimeSlotAvailableAsync(Guid dealerId, DateTime appointmentDate, TimeSpan startTime);
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(Guid dealerId);
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<IEnumerable<TestDriveAppointment>> GetAllAppointmentsAsync(Guid userId);
    }
}
