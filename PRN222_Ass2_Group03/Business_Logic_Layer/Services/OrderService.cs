using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Interfaces;
using DataAccess_Layer.Repositories.Interface;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;
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

        // 🔹 Danh sách tất cả đơn hàng của người dùng
        public async Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Select(MapToDTO).ToList();
        }

        // 🔹 Lịch sử đơn hàng (đã hoàn thành hoặc bị hủy)
        public async Task<List<OrderDTO>> GetOrderHistoryAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders
                .Where(o => o.Status == "Completed" || o.Status == "Cancelled")
                .Select(MapToDTO)
                .ToList();
        }

        // 🔹 Đơn hàng đang chờ xử lý
        public async Task<List<OrderDTO>> GetPendingOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders
                .Where(o => o.Status == "Pending" || o.Status == "Processing")
                .Select(MapToDTO)
                .ToList();
        }

        // 🔹 Chi tiết đơn hàng
        public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            return order == null ? null : MapToDTO(order);
        }

        // ✅ Map entity sang DTO
        private static OrderDTO MapToDTO(Order o)
        {
            return new OrderDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName,
                DealerId = o.DealerId,
                DealerName = o.Dealer?.Name,
                VehicleId = o.VehicleId,
                VehicleName = o.Vehicle?.Name,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                Notes = o.Notes,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            };
        }
        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return false;
            if (order.Status != "Processing") return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            return await _orderRepository.Update(order);
        }
    }
}


