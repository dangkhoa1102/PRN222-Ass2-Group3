
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerTestDriveAppointmentService _testDriveService;

        public CreateModel(IOrderService orderService, ICustomerTestDriveAppointmentService testDriveService)
        {
            _orderService = orderService;
            _testDriveService = testDriveService;
        }

        [BindProperty]
        public Guid SelectedDealerId { get; set; }

        [BindProperty]
        public Guid SelectedVehicleId { get; set; }

        [BindProperty]
        public string Notes { get; set; }

        public List<SelectListItem> DealersList { get; set; }
        public List<SelectListItem> VehiclesList { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync(Guid? vehicleId)
        {
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
                var newOrder = await _orderService.CreateOrderAsync(customerId, SelectedDealerId, SelectedVehicleId, Notes);
                SuccessMessage = "Order created successfully!";
                return RedirectToPage("/Orders/MyOrders");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to create order: " + ex.Message;
                return Page();
            }
        }
    }
}
