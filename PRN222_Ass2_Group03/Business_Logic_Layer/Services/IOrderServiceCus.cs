using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface IOrderServiceCus
    {
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<OrderDTO>> GetOrderHistoryAsync(Guid userId);
        Task<IEnumerable<OrderDTO>> GetPendingOrdersByUserIdAsync(Guid userId);
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId, string Notes);
        Task<OrderDTO> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<List<OrderDTO>> GetAllOrdersDTOAsync();
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<bool> UpdateOrderAsync(OrderDTO order);
        Task<bool> MarkDoneByCustomerAsync(Guid orderId, Guid customerId);
    }
}