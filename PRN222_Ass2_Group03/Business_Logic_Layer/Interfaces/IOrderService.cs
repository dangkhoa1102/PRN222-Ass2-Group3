using EVDealerDbContext.Models;

namespace Business_Logic_Layer.Interfaces
{
    public interface IOrderService
    {
        // Order CRUD
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Dealer?> GetFirstDealerAsync();


        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(Guid id);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);

        // Customer operations
        Task<IEnumerable<User>> SearchCustomersAsync(string keyword);
        Task<User?> GetCustomerByPhoneAsync(string phone);
        Task<User?> GetCustomerByIdAsync(Guid id);
        Task<User> CreateCustomerAsync(User user);

        // Vehicle operations
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(Guid id);

        // Business logic
        string GenerateOrderNumber();
        Task<decimal> CalculateTotalAmountAsync(Guid vehicleId, int quantity = 1);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, Guid userId, string? notes = null);
        Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(Guid orderId);
    }
}