using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class MyOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<Order> Orders { get; set; } = new();
        public string? CurrentUserId { get; set; }

        public MyOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            Guid userId = Guid.Parse(userIdStr);
            CurrentUserId = userIdStr;
            
            // Chỉ lấy các order chưa hoàn thành (Processing, Pending, Confirmed, Shipped, Complete)
            // "Complete" orders sẽ được hiển thị để customer có thể chuyển thành DONE
            // "DONE", "Completed", "Cancelled" orders sẽ không hiển thị ở đây (sẽ hiển thị trong Order History)
            var allOrders = await _orderService.GetOrdersByUserIdAsync(userId);
            Orders = allOrders.Where(o => 
                o.Status != "Completed" && 
                o.Status != "Cancelled" && 
                o.Status != "Done" &&
                o.Status != "DONE"
            ).ToList();

            return Page();
        }
    }
}