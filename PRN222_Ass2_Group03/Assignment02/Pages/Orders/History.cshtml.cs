using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class HistoryModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> OrderHistories { get; set; } = new();

        public HistoryModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            Guid userId = Guid.Parse(UserId!);
            OrderHistories = await _orderService.GetOrderHistoryAsync(userId);
            return Page();
        }
    }
}
