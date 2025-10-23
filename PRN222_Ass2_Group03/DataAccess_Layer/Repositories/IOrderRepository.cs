using EVDealerDbContext.Models;

namespace DataAccess_Layer.Repositories
{
    public interface IOrderRepository
    {
        // Basic CRUD
        Task<Dealer?> GetFirstActiveDealerAsync();

        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);

        // Business queries
        Task<bool> OrderExistsAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Order>> GetOrdersByDealerIdAsync(Guid dealerId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);

        // Statistics
        Task<int> GetTotalOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<Dictionary<string, int>> GetOrderCountByStatusAsync();
    }
}