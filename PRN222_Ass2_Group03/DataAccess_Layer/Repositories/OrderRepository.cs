using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess_Layer.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EVDealerSystemContext _context = new EVDealerSystemContext();
        public OrderRepository()
        {
          _context = new EVDealerSystemContext();
        }

        // ==================== BASIC CRUD ====================

        public async Task<Dealer?> GetFirstActiveDealerAsync()
        {
            return await _context.Dealers
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.CreatedAt)
                .FirstOrDefaultAsync();
        }




        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Include(o => o.OrderHistories)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Include(o => o.OrderHistories)
                        .ThenInclude(oh => oh.CreatedByNavigation)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Order> AddAsync(Order order)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                order.Id = Guid.NewGuid();
                order.CreatedAt = DateTime.Now;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateAsync(Order order)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                order.UpdatedAt = DateTime.Now;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                var order = await _context.Orders
                    .Include(o => o.OrderHistories)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order != null)
                {
                    if (order.OrderHistories != null && order.OrderHistories.Count > 0)
                    {
                        _context.OrderHistories.RemoveRange(order.OrderHistories);
                    }
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ==================== BUSINESS QUERIES ====================
        public async Task<bool> OrderExistsAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders.AnyAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByDealerIdAsync(Guid dealerId)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Where(o => o.DealerId == dealerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dealer)
                    .Include(o => o.Vehicle)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ==================== STATISTICS ====================
        public async Task<int> GetTotalOrdersCountAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .Where(o => o.Status != "Đã hủy")
                    .SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetOrderCountByStatusAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}