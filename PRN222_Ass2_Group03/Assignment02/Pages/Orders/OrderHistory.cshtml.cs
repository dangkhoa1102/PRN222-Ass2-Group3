
using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.OrderHistory
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
        public IEnumerable<EVDealerDbContext.Models.OrderHistory> OrderHistories { get; set; } = new List<EVDealerDbContext.Models.OrderHistory>();

        [BindProperty(SupportsGet = true)]
        public Guid? SelectedOrderId { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? orderId)
        {
            try
            {
                // Ki?m tra ??ng nh?p
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    TempData["ErrorMessage"] = "Please login to view order history!";
                    return RedirectToPage("/Login");
                }

                // L?y t?t c? orders (sau này có th? filter theo Staff n?u c?n)
                Orders = await _orderService.GetAllOrdersAsync();

                // N?u có ch?n order c? th?
                if (orderId.HasValue)
                {
                    SelectedOrderId = orderId.Value;
                    OrderHistories = await _orderService.GetOrderHistoryByOrderIdAsync(orderId.Value);
                }
                else if (Orders.Any())
                {
                    // M?c ??nh ch?n order ??u tiên
                    SelectedOrderId = Orders.First().Id;
                    OrderHistories = await _orderService.GetOrderHistoryByOrderIdAsync(SelectedOrderId.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading order history: {ex.Message}";
                return Page();
            }
        }
    }
}