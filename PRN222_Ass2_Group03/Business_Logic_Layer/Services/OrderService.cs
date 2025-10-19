using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // 🔹 Tất cả đơn hàng của user
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _orderRepository.GetByCustomerId(userId);
        }

        // 🔹 Lịch sử đơn hàng (đã hoàn thành hoặc hủy)
        public async Task<IEnumerable<Order>> GetOrderHistoryAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Where(o => o.Status == "Completed" || o.Status == "Cancelled");
        }

        // 🔹 Đơn hàng đang chờ xử lý
        public async Task<IEnumerable<Order>> GetPendingOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Where(o => o.Status == "Pending" || o.Status == "Processing");
        }

        // 🔹 Lấy chi tiết đơn hàng
        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _orderRepository.GetById(orderId);
        }

        // 🔹 Hủy đơn hàng
        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return false;

            // Chỉ cho phép hủy khi đang xử lý
            if (order.Status != "Processing") return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            return await _orderRepository.Update(order);
        }

    }
}
