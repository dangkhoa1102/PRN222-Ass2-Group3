using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        private OrderDTO ConvertToDTO(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? order.Customer?.Username ?? "Unknown",
                CustomerPhone = order.Customer?.Phone ?? "N/A",
                DealerId = order.DealerId,
                DealerName = order.Dealer?.Name ?? "Unknown",
                VehicleId = order.VehicleId,
                VehicleName = order.Vehicle?.Name ?? "Unknown",
                VehicleBrand = order.Vehicle?.Brand ?? "",
                VehicleModel = order.Vehicle?.Model ?? "",
                VehicleImage = order.Vehicle?.Images ?? "",
                TotalAmount = order.TotalAmount,
                Status = order.Status ?? "Unknown",
                PaymentStatus = order.PaymentStatus ?? "Unknown",
                Notes = order.Notes ?? "",
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        // 🔹 Tất cả đơn hàng của user
        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Select(ConvertToDTO);
        }

        // 🔹 Lịch sử đơn hàng (đã hoàn thành hoặc hủy)
        public async Task<IEnumerable<OrderDTO>> GetOrderHistoryAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Where(o => o.Status == "Completed" || o.Status == "Cancelled").Select(ConvertToDTO);
        }

        // 🔹 Đơn hàng đang chờ xử lý
        public async Task<IEnumerable<OrderDTO>> GetPendingOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByCustomerId(userId);
            return orders.Where(o => o.Status == "Pending" || o.Status == "Processing").Select(ConvertToDTO);
        }

        // 🔹 Lấy chi tiết đơn hàng với đầy đủ thông tin
        public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            return order != null ? ConvertToDTO(order) : null;
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
        public async Task<OrderDTO> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes)
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
                Notes = string.IsNullOrWhiteSpace(notes) ? "Không có ghi chú" : notes,
                Status = "Processing",
                PaymentStatus = "Unpaid",
                TotalAmount = total,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _orderRepository.Add(newOrder);
            return ConvertToDTO(newOrder);
        }
        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAll();
            return orders.Select(ConvertToDTO).ToList();
        }

        public async Task<List<OrderDTO>> GetAllOrdersDTOAsync()
        {
            var orders = await _orderRepository.GetAll();
            var orderDTOs = new List<OrderDTO>();

            foreach (var order in orders)
            {
                var orderDTO = new OrderDTO
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer?.FullName ?? order.Customer?.Username ?? "Unknown",
                    CustomerPhone = order.Customer?.Phone ?? "N/A",
                    DealerId = order.DealerId,
                    DealerName = order.Dealer?.Name ?? "Unknown",
                    VehicleId = order.VehicleId,
                    VehicleName = order.Vehicle?.Name ?? "Unknown",
                    VehicleBrand = order.Vehicle?.Brand ?? "",
                    VehicleModel = order.Vehicle?.Model ?? "",
                    VehicleImage = order.Vehicle?.Images ?? "",
                    TotalAmount = order.TotalAmount,
                    Status = order.Status ?? "Unknown",
                    PaymentStatus = order.PaymentStatus ?? "Unknown",
                    Notes = order.Notes ?? "",
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt
                };
                orderDTOs.Add(orderDTO);
            }

            return orderDTOs;
        }

        // 🔹 Cập nhật trạng thái đơn hàng
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null)
                return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            return await _orderRepository.Update(order);
        }

        // 🔹 Cập nhật toàn bộ đơn hàng
        public async Task<bool> UpdateOrderAsync(OrderDTO orderDto)
        {
            if (orderDto == null)
                return false;

            var order = await _orderRepository.GetById(orderDto.Id);
            if (order == null)
                return false;

            order.Status = orderDto.Status;
            order.PaymentStatus = orderDto.PaymentStatus;
            order.Notes = orderDto.Notes;
            order.UpdatedAt = DateTime.Now;
            return await _orderRepository.Update(order);
        }

    }
}