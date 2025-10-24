
using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class CreateModel : AuthenticatedPageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly ICustomerTestDriveAppointmentService _testDriveService;
        private readonly RealTimeNotificationService _notificationService;

        public CreateModel(IOrderServiceCus orderService, ICustomerTestDriveAppointmentService testDriveService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _testDriveService = testDriveService;
            _notificationService = notificationService;
        }

        [BindProperty]
        public Guid SelectedDealerId { get; set; }

        [BindProperty]
        public Guid SelectedVehicleId { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        [BindProperty]
        public DateTime? PreferredDeliveryDate { get; set; }

        public List<SelectListItem> DealersList { get; set; } = new();
        public List<SelectListItem> VehiclesList { get; set; } = new();
        public VehicleDTO? SelectedVehicle { get; set; }

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(Guid? vehicleId)
        {
            // Check authentication
            if (!IsAuthenticated)
            {
                return;
            }

            var dealers = await _testDriveService.GetAllDealersAsync();
            DealersList = dealers.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            var vehicles = await _testDriveService.GetAvailableVehiclesAsync(dealers.FirstOrDefault()?.Id ?? Guid.Empty);
            VehiclesList = vehicles.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name
            }).ToList();

            // Pre-select vehicle if vehicleId is provided
            if (vehicleId.HasValue)
            {
                SelectedVehicleId = vehicleId.Value;
                // Load vehicle details
                var allVehicles = await _testDriveService.GetAvailableVehiclesAsync(Guid.Empty);
                SelectedVehicle = allVehicles.FirstOrDefault(v => v.Id == vehicleId.Value);
            }
        }

        public async Task<IActionResult> OnGetGetVehicleInfoAsync(Guid vehicleId)
        {
            try
            {
                var allVehicles = await _testDriveService.GetAvailableVehiclesAsync(Guid.Empty);
                var vehicle = allVehicles.FirstOrDefault(v => v.Id == vehicleId);
                
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
            var customerIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerIdStr))
            {
                ErrorMessage = "Please log in to create an order.";
                return Page();
            }

            var customerId = Guid.Parse(customerIdStr);

            try
            {
                // Validate required fields
                if (SelectedDealerId == Guid.Empty)
                {
                    ErrorMessage = "Please select a dealer.";
                    return Page();
                }
                
                if (SelectedVehicleId == Guid.Empty)
                {
                    ErrorMessage = "Please select a vehicle.";
                    return Page();
                }

                // Validate preferred delivery date
                if (PreferredDeliveryDate.HasValue)
                {
                    var now = DateTime.Now;
                    if (PreferredDeliveryDate.Value <= now)
                    {
                        ErrorMessage = "Preferred delivery date cannot be in the past. Please select a future date.";
                        return Page();
                    }
                }

                var newOrder = await _orderService.CreateOrderAsync(customerId, SelectedDealerId, SelectedVehicleId, Notes ?? string.Empty);
                
                // Gửi notification real-time
                await _notificationService.NotifyOrderCreated(newOrder.OrderNumber, newOrder.CustomerName, newOrder.VehicleName);
                await _notificationService.NotifyPageReload("orders", "new_order");
                
                SuccessMessage = "Order created successfully!";
                return RedirectToPage("/Orders/MyOrders");
            }
            catch (Exception ex)
            {
                // Log detailed error for debugging
                Console.WriteLine($"Error creating order: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Check for specific database errors
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    ErrorMessage = $"Failed to create order: {ex.InnerException.Message}";
                }
                else
                {
                    ErrorMessage = "Failed to create order: " + ex.Message;
                }
                
                return Page();
            }
        }
    }
}
