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

        // ? X? lý khi ng??i dùng nh?n nút Cancel
        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // G?i hàm CancelOrderAsync trong Service
                bool result = await _orderService.CancelOrderAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "??n hàng ?ã ???c h?y thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không th? h?y ??n hàng này.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"?ã x?y ra l?i: {ex.Message}";
            }

            // Redirect l?i trang hi?n t?i ?? c?p nh?t danh sách
            return RedirectToPage();
        }
    }
}
