using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess_Layer.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EVDealerSystemContext _context;

        public OrderRepository(EVDealerSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAll()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Vehicle)
                .Include(o => o.Dealer)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByStatus(string status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Vehicle)
                .Include(o => o.Dealer)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByCustomerId(Guid customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Vehicle)
                .Include(o => o.Dealer)
                .Include(o => o.OrderHistories)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetById(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.TestDriveAppointments)
                .Include(o => o.Vehicle)
                .Include(o => o.Dealer)
                .Include(o => o.OrderHistories)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> Add(Order order)
        {
            _context.Orders.Add(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(Order order)
        {
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> AddOrderHistory(Guid orderId, string status, string notes, Guid createdBy)
        {
            var orderHistory = new OrderHistory
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Status = status,
                Notes = notes,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            _context.OrderHistories.Add(orderHistory);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<decimal?> GetVehiclePriceById(Guid vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Where(v => v.Id == vehicleId)
                .Select(v => v.Price)
                .FirstOrDefaultAsync();

            return vehicle;
        }
    }
}
