using Business_Logic_Layer.DTOs;
using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
        Task<List<OrderDTO>> GetOrderHistoryAsync(Guid userId);
        Task<List<OrderDTO>> GetPendingOrdersByUserIdAsync(Guid userId);
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId, string Notes);
        Task<Order> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);

    }
}
