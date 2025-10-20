using EVDealerDbContext.Models;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;

namespace Business_Logic_Layer.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly EVDealerSystemContext _context;

        public VehicleService(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .Where(v => v.IsActive == true)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(Guid id)
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id && v.IsActive == true);
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            vehicle.Id = Guid.NewGuid();
            vehicle.CreatedAt = DateTime.Now;
            vehicle.IsActive = true;

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existingVehicle == null)
            {
                throw new ArgumentException("Vehicle not found");
            }

            existingVehicle.Name = vehicle.Name;
            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.Price = vehicle.Price;
            existingVehicle.Description = vehicle.Description;
            existingVehicle.Specifications = vehicle.Specifications;
            existingVehicle.Images = vehicle.Images;
            existingVehicle.StockQuantity = vehicle.StockQuantity;

            await _context.SaveChangesAsync();
            return existingVehicle;
        }

        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return false;
            }

            // Soft delete - set IsActive to false instead of removing
            vehicle.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
