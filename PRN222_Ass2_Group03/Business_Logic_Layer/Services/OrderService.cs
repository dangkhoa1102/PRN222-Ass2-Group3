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
        public async Task<bool> CancelOrderAsync(Guid orderId, string Notes)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return false;

            if (order.Status != "Processing") return false;

            order.Status = "Cancelled";
            order.Notes = Notes; 
            order.UpdatedAt = DateTime.Now;

            return await _orderRepository.Update(order);
        }
        public async Task<Order> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes)
        {
            var now = DateTime.Now;
            string orderNumber = $"ORD-{now:yyyyMMdd-HHmm}";

            var vehiclePrice = await _orderRepository.GetVehiclePriceById(vehicleId);
            decimal total = vehiclePrice ?? 0;

            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,      
                CustomerId = customerId,
                DealerId = dealerId,
                VehicleId = vehicleId,
                Notes = notes,
                Status = "Processing",          
                PaymentStatus = "Unpaid",
                TotalAmount = total,         
                CreatedAt = now,
                UpdatedAt = now
            };

            await _orderRepository.Add(newOrder);
            return newOrder;
        }

    }
}
