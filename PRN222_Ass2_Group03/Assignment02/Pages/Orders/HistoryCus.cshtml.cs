using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class HistoryModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;

        public List<OrderDTO> OrderHistories { get; set; } = new();

        public HistoryModel(IOrderServiceCus orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            Guid userId = Guid.Parse(userIdStr);
            
            // Lấy các order đã hoàn thành (Completed, Complete, Cancelled, Done, DONE)
            var allOrders = await _orderService.GetOrdersByUserIdAsync(userId);
            OrderHistories = allOrders.Where(o => 
                string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(o.Status, "Complete", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(o.Status, "Done", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Status, "DONE", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            return Page();
        }
    }
}