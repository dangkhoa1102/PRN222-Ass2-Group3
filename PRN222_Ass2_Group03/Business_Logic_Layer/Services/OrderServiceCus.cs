using DataAccess_Layer.Repositories;
using EVDealerDbContext.Models;
using Business_Logic_Layer.DTOs;

namespace Business_Logic_Layer.Services
{
    public class OrderServiceCus : IOrderServiceCus
    {
        private readonly IOrderRepositoryCus _orderRepository;
        private readonly IVehicleService _vehicleService;

        public OrderServiceCus(IOrderRepositoryCus orderRepository, IVehicleService vehicleService)
        {
            _orderRepository = orderRepository;
            _vehicleService = vehicleService;
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

            var updateResult = await _orderRepository.Update(order);
            
            // If order cancellation is successful, increase stock quantity back
            if (updateResult)
            {
                try
                {
                    await _vehicleService.IncreaseStockQuantityAsync(order.VehicleId, 1);
                    Console.WriteLine($"Stock quantity increased back for vehicle {order.VehicleId} after order cancellation");
                }
                catch (Exception stockEx)
                {
                    Console.WriteLine($"Failed to increase stock back: {stockEx.Message}");
                    // Note: Order is already cancelled, so we just log the error
                }
            }

            return updateResult;
        }
        public async Task<OrderDTO> CreateOrderAsync(Guid customerId, Guid dealerId, Guid vehicleId, string notes)
        {
            try
            {
                var now = DateTime.Now;
                string orderNumber = $"ORD-{now:yyyyMMdd-HHmm}";

                // Validate that vehicle exists and get price
                var vehiclePrice = await _orderRepository.GetVehiclePriceById(vehicleId);
                if (vehiclePrice == null)
                {
                    throw new ArgumentException($"Vehicle with ID {vehicleId} not found.");
                }
                decimal total = vehiclePrice.Value;

                // Check stock quantity before creating order
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    throw new ArgumentException($"Vehicle with ID {vehicleId} not found.");
                }
                
                if (vehicle.StockQuantity <= 0)
                {
                    throw new InvalidOperationException($"Xe này đã hết hàng trong kho. Số lượng hiện tại: {vehicle.StockQuantity}");
                }

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

                var result = await _orderRepository.Add(newOrder);
                if (!result)
                {
                    throw new InvalidOperationException("Failed to save order to database.");
                }

                // Decrease stock quantity after successful order creation
                try
                {
                    await _vehicleService.DecreaseStockQuantityAsync(vehicleId, 1);
                    Console.WriteLine($"Stock quantity decreased for vehicle {vehicleId}");
                }
                catch (Exception stockEx)
                {
                    // If stock decrease fails, we should rollback the order
                    Console.WriteLine($"Failed to decrease stock: {stockEx.Message}");
                    // Note: In a production environment, you might want to implement transaction rollback here
                    throw new InvalidOperationException($"Order created but failed to update stock: {stockEx.Message}");
                }
                
                return ConvertToDTO(newOrder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateOrderAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
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

        // 🔹 Đánh dấu đơn hàng là hoàn thành bởi khách hàng
        public async Task<bool> MarkDoneByCustomerAsync(Guid orderId, Guid customerId)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null)
                return false;

            // Kiểm tra xem đơn hàng có thuộc về khách hàng này không
            if (order.CustomerId != customerId)
                return false;

            // Chỉ cho phép đánh dấu hoàn thành nếu đơn hàng đang ở trạng thái "Complete"
            if (order.Status != "Complete")
                return false;

            order.Status = "Done";
            order.UpdatedAt = DateTime.Now;
            return await _orderRepository.Update(order);
        }

    }
}