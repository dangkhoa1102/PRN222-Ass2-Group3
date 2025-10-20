using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class CreateModel : AuthenticatedPageModel
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
        public string Notes { get; set; } = string.Empty;

        public List<SelectListItem> DealersList { get; set; } = new();
        public List<SelectListItem> VehiclesList { get; set; } = new();

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsAuthenticated)
            {
                ErrorMessage = "Please log in to create an order.";
                return Page();
            }

            var customerId = Guid.Parse(UserId!);

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
