using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class ViewOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly RealTimeNotificationService _notificationService;

        public List<OrderDTO> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public ViewOrdersModel(IOrderServiceCus orderService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var userId = Guid.Parse(UserId!);
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);

            // Apply filters
            if (!string.IsNullOrEmpty(Status))
                orders = orders.Where(o => o.Status == Status).ToList();

            if (!string.IsNullOrEmpty(PaymentStatus))
                orders = orders.Where(o => o.PaymentStatus == PaymentStatus).ToList();

            if (FromDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= FromDate).ToList();

            if (ToDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= ToDate).ToList();

            // Sort by created date descending
            orders = orders.OrderByDescending(o => o.CreatedAt).ToList();

            Orders = Orders;
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
                // Owner-or-admin check
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

                // Check if order is already paid (for staff/admin)
                if (isPrivileged && string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã thanh toán.";
                    return RedirectToPage();
                }

                bool result = await _orderService.CancelOrderAsync(id, "Cancelled by user");

                if (result)
                {
                    // Gửi notification real-time khi order bị hủy
                    await _notificationService.NotifyOrderCancelled(order.OrderNumber, order.CustomerName, order.VehicleName);
                    await _notificationService.NotifyPageReload("orders", "order_cancelled");
                    
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
