using EVDealerDbContext.Models;

namespace Business_Logic_Layer.Services
{
    public interface IOrderService
    {
        // ==================== ORDER OPERATIONS ====================
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Dealer?> GetFirstDealerAsync();
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(Guid id);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, Guid userId, string? notes = null);

        // ==================== CUSTOMER OPERATIONS ====================
        Task<IEnumerable<User>> SearchCustomersAsync(string keyword);
        Task<User?> GetCustomerByPhoneAsync(string phone);
        Task<User?> GetCustomerByIdAsync(Guid id);
        Task<User?> CreateCustomerAsync(User user);

        // ==================== VEHICLE OPERATIONS ====================
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(Guid id);

        // ==================== BUSINESS LOGIC ====================
        string GenerateOrderNumber();
        Task<decimal> CalculateTotalAmountAsync(Guid vehicleId, int quantity = 1);

        // ==================== ORDER HISTORY ====================
        Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(Guid orderId);
        Task<IEnumerable<OrderHistory>> GetOrderHistoryByOrderIdAsync(Guid orderId);
    }
}