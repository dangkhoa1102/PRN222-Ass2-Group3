using Business_Logic_Layer.Interfaces;
using DataAccess_Layer.Repositories;
using EVDealerDbContext;
using EVDealerDbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace Business_Logic_Layer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // ==================== ORDER OPERATIONS ====================


        public async Task<Dealer?> GetFirstDealerAsync()
        {
            return await _orderRepository.GetFirstActiveDealerAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _orderRepository.GetOrderByOrderNumberAsync(orderNumber);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Generate order number nếu chưa có
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                order.OrderNumber = GenerateOrderNumber();
            }

            // Set default values
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.Now;

            if (string.IsNullOrEmpty(order.Status))
            {
                order.Status = "Đang xử lý";
            }

            if (string.IsNullOrEmpty(order.PaymentStatus))
            {
                order.PaymentStatus = "Chưa thanh toán";
            }

            // Lưu order
            var createdOrder = await _orderRepository.AddAsync(order);

            // Tạo OrderHistory để track
            await CreateOrderHistoryAsync(createdOrder.Id, createdOrder.Status,
                "Đơn hàng được tạo", order.CustomerId);

            return createdOrder;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                order.UpdatedAt = DateTime.Now;
                await _orderRepository.UpdateAsync(order);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            try
            {
                var orderExists = await _orderRepository.OrderExistsAsync(id);
                if (!orderExists)
                {
                    return false;
                }

                await _orderRepository.DeleteAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, Guid userId, string? notes = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.Status;
                order.Status = newStatus;
                order.UpdatedAt = DateTime.Now;

                await _orderRepository.UpdateAsync(order);

                // Tạo OrderHistory
                await CreateOrderHistoryAsync(orderId, newStatus,
                    notes ?? $"Thay đổi trạng thái từ '{oldStatus}' sang '{newStatus}'", userId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==================== CUSTOMER OPERATIONS ====================
        public async Task<IEnumerable<User>> SearchCustomersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<User>();
            }

            try
            {
                using var _context = new EVDealerSystemContext();
                keyword = keyword.ToLower();

                return await _context.Users
                    .Where(u => u.Role == "Customer" && u.IsActive == true &&
                        (u.FullName.ToLower().Contains(keyword) ||
                         u.Phone.Contains(keyword) ||
                         u.Email.ToLower().Contains(keyword)))
                    .ToListAsync();
            }
            catch
            {
                return new List<User>();
            }
        }

        public async Task<User?> GetCustomerByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == phone && u.Role == "Customer" && u.IsActive == true);
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                var user = await _context.Users.FindAsync(id);
                return user?.Role == "Customer" ? user : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> CreateCustomerAsync(User user)
        {
            try
            {
                using var _context = new EVDealerSystemContext();

                // Validate phone exists
                var phoneExists = await _context.Users.AnyAsync(u => u.Phone == user.Phone);
                if (phoneExists)
                {
                    throw new Exception("Số điện thoại đã tồn tại!");
                }

                // Set default values
                user.Id = Guid.NewGuid();
                user.Role = "Customer";
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                // Generate username từ phone nếu chưa có
                if (string.IsNullOrEmpty(user.Username))
                {
                    user.Username = user.Phone;
                }

                // Set default password nếu chưa có
                if (string.IsNullOrEmpty(user.Password))
                {
                    user.Password = "123456"; // TODO: Hash password
                }

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch
            {
                throw;
            }
        }

        // ==================== VEHICLE OPERATIONS ====================
        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Vehicles
                    .Where(v => v.IsActive == true && v.StockQuantity > 0)
                    .OrderBy(v => v.Brand)
                    .ThenBy(v => v.Model)
                    .ToListAsync();
            }
            catch
            {
                return new List<Vehicle>();
            }
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.Vehicles.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        // ==================== BUSINESS LOGIC ====================
        public string GenerateOrderNumber()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}";
        }

        public async Task<decimal> CalculateTotalAmountAsync(Guid vehicleId, int quantity = 1)
        {
            var vehicle = await GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return 0;
            }

            return vehicle.Price * quantity;
        }

        public async Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(Guid orderId)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                return await _context.OrderHistories
                    .Include(oh => oh.CreatedByNavigation)
                    .Where(oh => oh.OrderId == orderId)
                    .OrderByDescending(oh => oh.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                return new List<OrderHistory>();
            }
        }

        // ==================== PRIVATE HELPER ====================
        private async Task CreateOrderHistoryAsync(Guid orderId, string status, string notes, Guid userId)
        {
            try
            {
                using var _context = new EVDealerSystemContext();
                var history = new OrderHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Status = status,
                    Notes = notes,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                };

                await _context.OrderHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Log error but don't throw - history is optional
            }
        }
    }
}