using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync();
        Task<VehicleDTO?> GetVehicleByIdAsync(Guid id);
        Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicle);
        Task<VehicleDTO> UpdateVehicleAsync(VehicleDTO vehicle);
        Task<bool> DeleteVehicleAsync(Guid id);
        Task<bool> DecreaseStockQuantityAsync(Guid vehicleId, int quantity = 1);
        Task<bool> IncreaseStockQuantityAsync(Guid vehicleId, int quantity = 1);
    }
}
