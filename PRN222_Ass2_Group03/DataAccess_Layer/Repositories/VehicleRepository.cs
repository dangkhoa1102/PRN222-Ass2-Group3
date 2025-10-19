using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess_Layer.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly EVDealerSystemContext _context;

        public VehicleRepository(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id)
        {
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .Where(v => v.IsActive == true && v.StockQuantity > 0)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleWithOrdersAsync(Guid id)
        {
            return await _context.Vehicles
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}