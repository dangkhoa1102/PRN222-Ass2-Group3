using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid userId);
        Task<IEnumerable<Order>> GetPendingOrdersByUserIdAsync(Guid userId);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId);
    }
}
