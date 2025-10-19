using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class PendingOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IEnumerable<Order> PendingOrders { get; set; } = Enumerable.Empty<Order>();

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

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                bool result = await _orderService.CancelOrderAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng này.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
