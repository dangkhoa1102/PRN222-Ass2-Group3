using EVDealerDbContext.Models;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly EVDealerSystemContext _context;

        public VehicleService(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.IsActive == true)
                .OrderBy(v => v.Name)
                .ToListAsync();
                
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

        public async Task<VehicleDTO?> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id && v.IsActive == true);
                
            if (vehicle == null) return null;
            
            return new VehicleDTO
            {
                Id = vehicle.Id,
                Name = vehicle.Name,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year ?? 0,
                Price = vehicle.Price,
                Description = vehicle.Description,
                Specifications = vehicle.Specifications,
                Images = vehicle.Images,
                StockQuantity = vehicle.StockQuantity ?? 0,
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedAt
            };
        }

        public async Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicleDto)
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Name = vehicleDto.Name,
                Brand = vehicleDto.Brand,
                Model = vehicleDto.Model,
                Year = vehicleDto.Year,
                Price = vehicleDto.Price,
                Description = vehicleDto.Description,
                Specifications = vehicleDto.Specifications,
                Images = vehicleDto.Images,
                StockQuantity = vehicleDto.StockQuantity,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            
            vehicleDto.Id = vehicle.Id;
            vehicleDto.CreatedAt = vehicle.CreatedAt;
            vehicleDto.IsActive = vehicle.IsActive;
            return vehicleDto;
        }

        public async Task<VehicleDTO> UpdateVehicleAsync(VehicleDTO vehicleDto)
        {
            var existingVehicle = await _context.Vehicles.FindAsync(vehicleDto.Id);
            if (existingVehicle == null)
            {
                throw new ArgumentException("Vehicle not found");
            }

            existingVehicle.Name = vehicleDto.Name;
            existingVehicle.Brand = vehicleDto.Brand;
            existingVehicle.Model = vehicleDto.Model;
            existingVehicle.Year = vehicleDto.Year;
            existingVehicle.Price = vehicleDto.Price;
            existingVehicle.Description = vehicleDto.Description;
            existingVehicle.Specifications = vehicleDto.Specifications;
            existingVehicle.Images = vehicleDto.Images;
            existingVehicle.StockQuantity = vehicleDto.StockQuantity;

            await _context.SaveChangesAsync();
            
            return new VehicleDTO
            {
                Id = existingVehicle.Id,
                Name = existingVehicle.Name,
                Brand = existingVehicle.Brand,
                Model = existingVehicle.Model,
                Year = existingVehicle.Year ?? 0,
                Price = existingVehicle.Price,
                Description = existingVehicle.Description,
                Specifications = existingVehicle.Specifications,
                Images = existingVehicle.Images,
                StockQuantity = existingVehicle.StockQuantity ?? 0,
                IsActive = existingVehicle.IsActive,
                CreatedAt = existingVehicle.CreatedAt
            };
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

        public async Task<bool> DecreaseStockQuantityAsync(Guid vehicleId, int quantity = 1)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            if (vehicle.StockQuantity < quantity)
            {
                throw new InvalidOperationException($"Không đủ số lượng xe trong kho. Số lượng hiện tại: {vehicle.StockQuantity}");
            }

            vehicle.StockQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
