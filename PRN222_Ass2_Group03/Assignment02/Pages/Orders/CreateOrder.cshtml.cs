using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Assignment02.Pages.Orders
{
    public class CreateOrderModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly IDealerService _dealerService;

        [BindProperty]
        public CreateOrderViewModel OrderViewModel { get; set; } = new();

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
        public List<SelectListItem> Dealers { get; set; } = new();

        public CreateOrderModel(IOrderService orderService, IUserService userService, IVehicleService vehicleService, IDealerService dealerService)
        {
            _orderService = orderService;
            _userService = userService;
            _vehicleService = vehicleService;
            _dealerService = dealerService;
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
                // Create the order using selected dealer
                var order = await _orderService.CreateOrderAsync(
                    OrderViewModel.CustomerId,
                    OrderViewModel.DealerId,
                    OrderViewModel.VehicleId,
                    OrderViewModel.Notes ?? string.Empty
                );

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
    }
}
