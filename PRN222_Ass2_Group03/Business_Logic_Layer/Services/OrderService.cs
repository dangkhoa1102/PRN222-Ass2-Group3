using Business_Logic_Layer.DTOs;
using DataAccess_Layer.Repositories;
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
                CustomerName = o.Customer?.FullName ?? string.Empty,
                DealerId = o.DealerId,
                DealerName = o.Dealer?.Name ?? string.Empty,
                VehicleId = o.VehicleId,
                VehicleName = o.Vehicle?.Name ?? string.Empty,
                VehicleBrand = o.Vehicle?.Brand ?? string.Empty,
                VehicleModel = o.Vehicle?.Model ?? string.Empty,
                VehicleImage = o.Vehicle?.Images ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                Notes = o.Notes,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            };
        }
        public async Task<bool> CancelOrderAsync(Guid orderId, string Notes)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return false;
            if (order.Status != "Processing") return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            order.Notes = Notes;

            return await _orderRepository.Update(order);
        }

        public async Task<Order> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes)
        {
            // Get vehicle price
            var vehiclePrice = await _orderRepository.GetVehiclePriceById(vehicleId);
            if (vehiclePrice == null)
            {
                throw new InvalidOperationException("Vehicle not found or price unavailable");
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                DealerId = dealerId,
                VehicleId = vehicleId,
                TotalAmount = vehiclePrice.Value,
                Status = "Pending",
                PaymentStatus = "Pending",
                Notes = notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _orderRepository.Add(order);
            if (!success)
            {
                throw new InvalidOperationException("Failed to create order");
            }

            return order;
        }

        private string GenerateOrderNumber()
        {
            return "ORD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        // 🔹 Get all orders (for admin)
        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAll();
            return orders.Select(MapToDTO).ToList();
        }

        // 🔹 Update order status (for admin)
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            return await _orderRepository.Update(order);
        }
    }
}


