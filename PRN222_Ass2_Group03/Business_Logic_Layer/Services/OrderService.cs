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
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAll();
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
        public async Task<bool> UpdateOrderAsync(Order order)
        {
            if (order == null)
                return false;

            order.UpdatedAt = DateTime.Now;
            return await _orderRepository.Update(order);
        }

    }
}