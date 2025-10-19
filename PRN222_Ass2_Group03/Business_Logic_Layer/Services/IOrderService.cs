//using Business_Logic_Layer.DTOs;

//namespace Business_Logic_Layer.Interfaces
//{
//    public interface IOrderService
//    {
//        Task<List<OrderDTO>> GetAllOrders();
//        Task<List<OrderDTO>> GetByStatus(string status);
//        Task<List<OrderDTO>> GetCustomerOrders(Guid customerId);
//        Task<OrderDTO?> GetOrderById(Guid id);
//        Task<bool> CreateOrder(CreateOrderDTO dto);
//        Task<bool> ConfirmOrder(Guid orderId, Guid staffId);
//        Task<bool> CompletePayment(Guid orderId, Guid customerId);
//        Task<bool> UpdateOrder(OrderDTO dto);
//        Task<bool> DeleteOrder(Guid id);
//        Task<bool> RejectOrder(Guid orderId, Guid customerId);
//    }
//}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
        Task<List<OrderDTO>> GetOrderHistoryAsync(Guid userId);
        Task<List<OrderDTO>> GetPendingOrdersByUserIdAsync(Guid userId);
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
        Task<bool> CancelOrderAsync(Guid orderId);

    }
}


