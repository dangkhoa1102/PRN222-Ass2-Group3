//using Business_Logic_Layer.DTOs;
//using Business_Logic_Layer.Interfaces;
//using DataAccess_Layer.Models;
//using DataAccess_Layer.Repositories.Interface;

//namespace Business_Logic_Layer.Services
//{
//    public class OrderService : IOrderService
//    {
//        private readonly IOrderRepository _orderRepository;
//        private readonly IVehicleRepository _vehicleRepository;
//        private readonly IDealerRepository _dealerRepository;

//        public OrderService(IOrderRepository orderRepository,
//                            IVehicleRepository vehicleRepository,
//                            IDealerRepository dealerRepository)
//        {
//            _orderRepository = orderRepository;
//            _vehicleRepository = vehicleRepository;
//            _dealerRepository = dealerRepository;
//        }

//        public async Task<List<OrderDTO>> GetAllOrders()
//        {
//            var orders = await _orderRepository.GetAll();
//            return orders.Select(MapToDTO).ToList();
//        }

//        public async Task<List<OrderDTO>> GetByStatus(string status)
//        {
//            var orders = await _orderRepository.GetByStatus(status);
//            return orders.Select(MapToDTO).ToList();
//        }

//        public async Task<List<OrderDTO>> GetCustomerOrders(Guid customerId)
//        {
//            var orders = await _orderRepository.GetByCustomerId(customerId);
//            return orders.Select(MapToDTO).ToList();
//        }

//        public async Task<OrderDTO?> GetOrderById(Guid id)
//        {
//            var order = await _orderRepository.GetById(id);
//            return order == null ? null : MapToDTO(order);
//        }

//        public async Task<bool> CreateOrder(CreateOrderDTO dto)
//        {
//            var vehicle = await _vehicleRepository.GetById(dto.VehicleId);
//            if (vehicle == null || vehicle.StockQuantity <= 0) return false;

//            var dealers = await _dealerRepository.GetAll();
//            if (!dealers.Any()) return false;

//            var order = new Order
//            {
//                Id = Guid.NewGuid(),
//                OrderNumber = GenerateOrderNumber(),
//                CustomerId = dto.CustomerId,
//                DealerId = dealers.First().Id,
//                VehicleId = dto.VehicleId,
//                TotalAmount = vehicle.Price,
//                Status = "Processing",
//                PaymentStatus = "Unpaid",
//                Notes = dto.Notes,
//                CreatedAt = DateTime.Now,
//                UpdatedAt = DateTime.Now
//            };

//            var success = await _orderRepository.Add(order);
//            if (success)
//            {
//                await _orderRepository.AddOrderHistory(order.Id, "Processing", "Đơn hàng được tạo", dto.CustomerId);
//                vehicle.StockQuantity -= 1;
//                await _vehicleRepository.UpdateAsync(vehicle);
//            }
//            return success;
//        }

//        public async Task<bool> ConfirmOrder(Guid orderId, Guid staffId)
//        {
//            var order = await _orderRepository.GetById(orderId);
//            if (order == null || order.Status != "Processing") return false;

//            order.Status = "Completed";
//            order.UpdatedAt = DateTime.UtcNow;

//            var success = await _orderRepository.Update(order);
//            if (success)
//                await _orderRepository.AddOrderHistory(orderId, "Completed", "Xác nhận hoàn tất bởi nhân viên", staffId);

//            return success;
//        }

//        public async Task<bool> RejectOrder(Guid orderId, Guid customerId)
//        {
//            var order = await _orderRepository.GetById(orderId);
//            if (order == null || order.Status != "Processing") return false;

//            order.Status = "Cancelled";
//            order.UpdatedAt = DateTime.UtcNow;

//            var success = await _orderRepository.Update(order);
//            if (success)
//                await _orderRepository.AddOrderHistory(order.Id, "Cancelled", "Khách hàng từ chối đơn hàng", customerId);

//            return success;
//        }

//        public async Task<bool> CompletePayment(Guid orderId, Guid customerId)
//        {
//            var order = await _orderRepository.GetById(orderId);
//            if (order == null || order.Status != "Processing") return false;

//            order.Status = "Completed";
//            order.PaymentStatus = "Paid";
//            order.UpdatedAt = DateTime.UtcNow;

//            var success = await _orderRepository.Update(order);
//            if (success)
//            {
//                var vehicle = await _vehicleRepository.GetById(order.VehicleId);
//                if (vehicle != null && vehicle.StockQuantity.HasValue)
//                {
//                    vehicle.StockQuantity -= 1;
//                    await _vehicleRepository.UpdateAsync(vehicle);
//                }
//                await _orderRepository.AddOrderHistory(orderId, "Completed", "Khách hàng đã thanh toán", customerId);
//            }
//            return success;
//        }

//        public async Task<bool> UpdateOrder(OrderDTO dto)
//        {
//            var order = await _orderRepository.GetById(dto.Id);
//            if (order == null) return false;

//            order.Status = dto.Status;
//            order.TotalAmount = dto.TotalAmount;
//            order.UpdatedAt = DateTime.UtcNow;

//            return await _orderRepository.Update(order);
//        }

//        public async Task<bool> DeleteOrder(Guid id) => await _orderRepository.Delete(id);

//        private static OrderDTO MapToDTO(Order o)
//        {
//            return new OrderDTO
//            {
//                Id = o.Id,
//                OrderNumber = o.OrderNumber,
//                CustomerId = o.CustomerId,
//                CustomerName = o.Customer?.FullName,
//                DealerId = o.DealerId,
//                DealerName = o.Dealer?.Name,
//                VehicleId = o.VehicleId,
//                VehicleName = o.Vehicle?.Name,
//                TotalAmount = o.TotalAmount,
//                Status = o.Status,
//                PaymentStatus = o.PaymentStatus,
//                Notes = o.Notes,
//                CreatedAt = o.CreatedAt,
//                UpdatedAt = o.UpdatedAt
//            };
//        }

//        private string GenerateOrderNumber() =>
//            $"ORD-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
//    }
//}



using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Interfaces;
using DataAccess_Layer.Repositories.Interface;
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
    }
}


