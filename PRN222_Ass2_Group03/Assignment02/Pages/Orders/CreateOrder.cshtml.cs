using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class CreateOrderModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;
        private readonly RealTimeNotificationService _notificationService;

        [BindProperty]
        public CreateOrderViewModel OrderViewModel { get; set; } = new();

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
        public List<SelectListItem> Dealers { get; set; } = new();

        public CreateOrderModel(IOrderServiceCus orderService, IUserService userService, IVehicleService vehicleService, IDealerService dealerService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _userService = userService;
            _vehicleService = vehicleService;
            _dealerService = dealerService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync(Guid? vehicleId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            var currentUserRole = HttpContext.Session.GetString("Role");
            var isStaffOrAdmin = string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(currentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);

            if (!isStaffOrAdmin)
            {
                return Forbid();
            }

            await LoadDropdownData();
            
            // Pre-select vehicle if vehicleId is provided
            if (vehicleId.HasValue)
            {
                OrderViewModel.VehicleId = vehicleId.Value;
            }
            
            return Page();
        }

        public async Task<IActionResult> OnGetGetVehicleInfoAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                
                if (vehicle != null)
                {
                    return new JsonResult(new
                    {
                        id = vehicle.Id,
                        name = vehicle.Name,
                        brand = vehicle.Brand,
                        model = vehicle.Model,
                        year = vehicle.Year,
                        price = vehicle.Price,
                        images = vehicle.Images
                    });
                }
                
                return new JsonResult(new { error = "Vehicle not found" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            var currentUserRole = HttpContext.Session.GetString("Role");
            var isStaffOrAdmin = string.Equals(currentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(currentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);

            if (!isStaffOrAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return Page();
            }

            try
            {
                // Validate preferred delivery date
                if (OrderViewModel.PreferredDeliveryDate.HasValue)
                {
                    var now = DateTime.Now;
                    if (OrderViewModel.PreferredDeliveryDate.Value <= now)
                    {
                        TempData["ErrorMessage"] = "Preferred delivery date cannot be in the past. Please select a future date.";
                        await LoadDropdownData();
                        return Page();
                    }
                }

                // Create the order using selected dealer
                var order = await _orderService.CreateOrderAsync(
                    OrderViewModel.CustomerId,
                    OrderViewModel.DealerId,
                    OrderViewModel.VehicleId,
                    OrderViewModel.Notes ?? string.Empty
                );

                // Gửi notification real-time
                await _notificationService.NotifyOrderCreated(order.OrderNumber, order.CustomerName, order.VehicleName);
                await _notificationService.NotifyPageReload("orders", "new_order");

                TempData["SuccessMessage"] = $"Đơn hàng đã được tạo thành công với mã: {order.OrderNumber}";
                return RedirectToPage("/Admin/ManageOrders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo đơn hàng: {ex.Message}";
                await LoadDropdownData();
                return Page();
            }
        }

        private async Task LoadDropdownData()
        {
            try
            {
                // Load customers
                var customers = await _userService.GetCustomersAsync();
                Customers = customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FullName} ({c.Username}) - {c.Phone ?? "N/A"}"
                }).ToList();

                // Load vehicles
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                Vehicles = vehicles.Where(v => v.IsActive == true)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Name} - {v.Brand} {v.Model} ({v.Year}) - {v.Price:N0} ₫"
                    }).ToList();

                // Load dealers
                var dealers = await _dealerService.GetAllDealersAsync();
                Dealers = dealers.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Name} - {d.Address ?? "N/A"}"
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"Error loading dropdown data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Set error message for UI
                TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                
                // Initialize empty lists to prevent null reference
                Customers = new List<SelectListItem>();
                Vehicles = new List<SelectListItem>();
                Dealers = new List<SelectListItem>();
            }
        }
    }

    public class CreateOrderViewModel
    {
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid DealerId { get; set; }
        public string? Notes { get; set; }
        public DateTime? PreferredDeliveryDate { get; set; }
    }
}
