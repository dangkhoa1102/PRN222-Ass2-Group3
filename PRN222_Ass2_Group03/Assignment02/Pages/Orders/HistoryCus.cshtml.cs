using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class HistoryModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<Order> OrderHistories { get; set; } = new();

        public HistoryModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            Guid userId = Guid.Parse(userIdStr);
            
            // Lấy các order đã hoàn thành (Completed, Cancelled, Done)
            var allOrders = await _orderService.GetOrdersByUserIdAsync(userId);
            OrderHistories = allOrders.Where(o => 
                o.Status == "Completed" || 
                o.Status == "Cancelled" || 
                o.Status == "Done"
            ).ToList();

            return Page();
        }
    }
}