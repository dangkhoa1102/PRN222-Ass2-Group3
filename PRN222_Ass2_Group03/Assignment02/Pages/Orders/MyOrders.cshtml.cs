using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Order
{
    public class MyOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> Orders { get; set; } = new();

        public MyOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            Guid userId = Guid.Parse(userIdStr);
            Orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Page();
        }
    }
}
