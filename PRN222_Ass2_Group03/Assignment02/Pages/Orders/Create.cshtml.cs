using Business_Logic_Layer.Interfaces;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Assignment02.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IOrderService _orderService;

        public CreateModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty]
        public OrderInputModel Input { get; set; } = new();

        public IEnumerable<Vehicle> AvailableVehicles { get; set; } = new List<Vehicle>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load danh sách xe còn hàng (StockQuantity > 0)
                AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();

                // Khởi tạo giá trị mặc định cho PaymentStatus
                Input.PaymentStatus = "Chưa thanh toán";

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Xóa validation errors cho CustomerName, CustomerPhone và CustomerEmail nếu đã chọn CustomerId
                if (Input.CustomerId.HasValue && Input.CustomerId.Value != Guid.Empty)
                {
                    ModelState.Remove("Input.CustomerName");
                    ModelState.Remove("Input.CustomerPhone");
                    ModelState.Remove("Input.CustomerEmail");
                }
                else
                {
                    // Nếu KHÔNG chọn khách hàng cũ, validate thông tin khách hàng mới
                    if (string.IsNullOrWhiteSpace(Input.CustomerName))
                    {
                        ModelState.AddModelError("Input.CustomerName", "Tên khách hàng là bắt buộc");
                    }
                    else if (Input.CustomerName.Trim().Length < 2 || Input.CustomerName.Trim().Length > 100)
                    {
                        ModelState.AddModelError("Input.CustomerName", "Tên phải từ 2-100 ký tự");
                    }

                    if (string.IsNullOrWhiteSpace(Input.CustomerPhone))
                    {
                        ModelState.AddModelError("Input.CustomerPhone", "Số điện thoại là bắt buộc");
                    }
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(Input.CustomerPhone.Trim(), @"^0\d{9,10}$"))
                    {
                        ModelState.AddModelError("Input.CustomerPhone", "Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số");
                    }

                    if (!string.IsNullOrWhiteSpace(Input.CustomerEmail) &&
                        !System.Text.RegularExpressions.Regex.IsMatch(Input.CustomerEmail.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Input.CustomerEmail", "Email không hợp lệ");
                    }
                }

                // Validate form
                if (!ModelState.IsValid)
                {
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    ErrorMessage = "Vui lòng điền đầy đủ thông tin bắt buộc.";
                    return Page();
                }

                // 1. Xử lý thông tin khách hàng (tìm hoặc tạo mới)
                User? customer = await HandleCustomerInformationAsync();
                if (customer == null)
                {
                    ErrorMessage = "Không thể xử lý thông tin khách hàng. Vui lòng thử lại.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // 2. Kiểm tra xe còn trong kho không
                var vehicle = await _orderService.GetVehicleByIdAsync(Input.VehicleId);
                if (vehicle == null)
                {
                    ErrorMessage = "Xe không tồn tại. Vui lòng chọn xe khác.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                if (vehicle.StockQuantity <= 0)
                {
                    ErrorMessage = $"Xe {vehicle.Brand} {vehicle.Model} đã hết hàng. Vui lòng chọn xe khác.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // 3. Validate TotalAmount khớp với giá xe
                if (Input.TotalAmount != vehicle.Price)
                {
                    ErrorMessage = "Tổng tiền không khớp với giá xe. Vui lòng thử lại.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // 4. Lấy DealerId từ session (hoặc mặc định)
                var dealerId = GetCurrentDealerId();

                // ✅ NẾU KHÔNG CÓ TRONG SESSION, LẤY DEALER ĐẦU TIÊN TỪ DB
                if (dealerId == Guid.Empty)
                {
                    var firstDealer = await _orderService.GetFirstDealerAsync();
                    if (firstDealer == null)
                    {
                        ErrorMessage = "Không tìm thấy dealer trong hệ thống. Vui lòng liên hệ quản trị viên.";
                        AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                        return Page();
                    }
                    dealerId = firstDealer.Id;
                }

                // 5. Tạo đơn hàng mới
                var order = new Order
                {
                    OrderNumber = _orderService.GenerateOrderNumber(), // VD: ORD-20250118-001
                    CustomerId = customer.Id,
                    VehicleId = Input.VehicleId,
                    DealerId = dealerId,
                    TotalAmount = Input.TotalAmount,
                    Status = "confirmed", // Trạng thái mặc định: đã xác nhận
                    PaymentStatus = Input.PaymentStatus, // "Chưa thanh toán" hoặc "Đã thanh toán"
                    Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
                    CreatedAt = DateTime.Now
                };

                // 6. Lưu đơn hàng vào database
                var createdOrder = await _orderService.CreateOrderAsync(order);

                if (createdOrder == null)
                {
                    ErrorMessage = "Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // 7. ✅ THÀNH CÔNG → REDIRECT VỀ INDEX
                SuccessMessage = $"✓ Đơn hàng {createdOrder.OrderNumber} đã được tạo thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                return Page();
            }
        }

        /// <summary>
        /// Xử lý thông tin khách hàng - CHỈ 2 TRƯỜNG HỢP
        /// </summary>
        private async Task<User?> HandleCustomerInformationAsync()
        {
            try
            {
                // TRƯỜNG HỢP 1: Đã chọn khách hàng từ dropdown (có CustomerId)
                // → Lấy thông tin khách hàng đó luôn, KHÔNG CẦN điền form
                if (Input.CustomerId.HasValue && Input.CustomerId.Value != Guid.Empty)
                {
                    var existingCustomer = await _orderService.GetCustomerByIdAsync(Input.CustomerId.Value);
                    if (existingCustomer != null)
                    {
                        return existingCustomer;
                    }
                }

                // TRƯỜNG HỢP 2: Không chọn từ dropdown (CustomerId = null)
                // → Tạo khách hàng MỚI với thông tin đã điền

                // Validate thông tin bắt buộc
                if (string.IsNullOrWhiteSpace(Input.CustomerName) ||
                    string.IsNullOrWhiteSpace(Input.CustomerPhone))
                {
                    ModelState.AddModelError("", "Tên và số điện thoại khách hàng là bắt buộc.");
                    return null;
                }

                // Tạo khách hàng mới
                var newCustomer = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = Input.CustomerName.Trim(),
                    Phone = Input.CustomerPhone.Trim(),
                    Email = string.IsNullOrWhiteSpace(Input.CustomerEmail) ? null : Input.CustomerEmail.Trim(),
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var createdCustomer = await _orderService.CreateCustomerAsync(newCustomer);
                return createdCustomer;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi xử lý khách hàng: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// API Handler: Tìm kiếm khách hàng theo tên hoặc SĐT
        /// </summary>
        public async Task<IActionResult> OnGetSearchCustomersAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return new JsonResult(new { results = Array.Empty<object>() });
                }

                var customers = await _orderService.SearchCustomersAsync(term.Trim());

                var results = customers.Select(c => new
                {
                    id = c.Id,
                    text = $"{c.FullName} - {c.Phone}",
                    fullName = c.FullName,
                    phone = c.Phone,
                    email = c.Email ?? ""
                }).ToList();

                return new JsonResult(new { results });
            }
            catch (Exception)
            {
                return new JsonResult(new { results = Array.Empty<object>() });
            }
        }

        /// <summary>
        /// API Handler: Lấy thông tin chi tiết xe
        /// </summary>
        public async Task<IActionResult> OnGetVehicleInfoAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await _orderService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound(new { error = "Không tìm thấy xe" });
                }

                return new JsonResult(new
                {
                    id = vehicle.Id,
                    name = $"{vehicle.Brand} {vehicle.Model}",
                    price = vehicle.Price,
                    stock = vehicle.StockQuantity,
                    specifications = vehicle.Specifications
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy DealerId hiện tại từ Session
        /// </summary>
        private Guid GetCurrentDealerId()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (!string.IsNullOrEmpty(dealerId) && Guid.TryParse(dealerId, out Guid id))
            {
                return id;
            }

            // ✅ Trả về Guid.Empty thay vì redirect
            // Backend sẽ tự động gán dealer mặc định
            return Guid.Empty;
        }

        /// <summary>
        /// Input Model cho form tạo đơn hàng
        /// </summary>
        public class OrderInputModel
        {
            /// <summary>
            /// ID khách hàng (nếu chọn từ dropdown)
            /// </summary>
            public Guid? CustomerId { get; set; }

            /// <summary>
            /// Tên khách hàng (chỉ bắt buộc khi tạo mới - không dùng validation attribute)
            /// </summary>
            [Display(Name = "Tên khách hàng")]
            public string? CustomerName { get; set; }

            /// <summary>
            /// Số điện thoại (chỉ bắt buộc khi tạo mới - không dùng validation attribute)
            /// </summary>
            [Display(Name = "Số điện thoại")]
            public string? CustomerPhone { get; set; }

            /// <summary>
            /// Email (không bắt buộc)
            /// </summary>
            [Display(Name = "Email")]
            public string? CustomerEmail { get; set; }

            /// <summary>
            /// ID xe được chọn (bắt buộc)
            /// </summary>
            [Required(ErrorMessage = "Vui lòng chọn xe")]
            [Display(Name = "Xe")]
            public Guid VehicleId { get; set; }

            /// <summary>
            /// Tổng tiền (tự động tính từ giá xe)
            /// </summary>
            [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
            [Display(Name = "Tổng tiền")]
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// Trạng thái thanh toán: "Chưa thanh toán" hoặc "Đã thanh toán"
            /// </summary>
            [Required(ErrorMessage = "Vui lòng chọn trạng thái thanh toán")]
            [Display(Name = "Trạng thái thanh toán")]
            public string PaymentStatus { get; set; } = "Chưa thanh toán";

            /// <summary>
            /// Ghi chú (không bắt buộc, tối đa 500 ký tự)
            /// </summary>
            [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
            [Display(Name = "Ghi chú")]
            public string? Notes { get; set; }
        }
    }
}