using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class PendingOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> PendingOrders { get; set; } = new();

        public PendingOrdersModel(IOrderService orderService)
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
            PendingOrders = await _orderService.GetPendingOrdersByUserIdAsync(userId);
            return Page();
        }

        // Handle cancel click
        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToPage("/Login");
                }

                // Owner-or-admin check before cancelling
                var order = await _orderService.GetOrderByIdAsync(id);
                var isPrivileged = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToPage();
                }
                if (!isPrivileged && order.CustomerId.ToString() != userIdStr)
                {
                    return Forbid();
                }

                bool result = await _orderService.CancelOrderAsync(id, "Cancelled by user");

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

            // Refresh current page to update list
            return RedirectToPage();
        }
    }
}
