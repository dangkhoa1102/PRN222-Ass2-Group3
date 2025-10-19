using EVDealerDbContext.Models;

namespace DataAccess_Layer.Repositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<Vehicle?> GetByIdAsync(Guid id);
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task<Vehicle?> GetVehicleWithOrdersAsync(Guid id);
        Task UpdateAsync(Vehicle vehicle);



    }
}