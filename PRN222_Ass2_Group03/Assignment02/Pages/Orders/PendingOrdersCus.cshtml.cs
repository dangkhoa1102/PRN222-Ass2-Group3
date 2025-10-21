
using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class PendingOrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IEnumerable<OrderDTO> PendingOrders { get; set; } = Enumerable.Empty<OrderDTO>();

        public PendingOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ✅ Lấy danh sách đơn đang xử lý
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

        // ✅ Nhận thêm tham số Notes từ form
        public async Task<IActionResult> OnPostCancelAsync(Guid id, string Notes)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Login");
            }

            // 🔸 Kiểm tra người dùng có nhập lý do không
            if (string.IsNullOrWhiteSpace(Notes))
            {
                TempData["ErrorMessage"] = "Bạn phải nhập lý do hủy đơn hàng.";
                return RedirectToPage();
            }

            try
            {
                // ✅ Gọi service mới có tham số Notes
                bool result = await _orderService.CancelOrderAsync(id, Notes);

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
