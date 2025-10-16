using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Order
{
    public class PendingOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> PendingOrders { get; set; } = new();

        public PendingOrdersModel(IOrderService orderService)
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
            PendingOrders = await _orderService.GetPendingOrdersByUserIdAsync(userId);
            return Page();
        }
    }
}
