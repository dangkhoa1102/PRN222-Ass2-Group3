using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class PaymentModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly RealTimeNotificationService _notificationService;

        public OrderDTO? Order { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentMethod { get; set; } = string.Empty;

        public PaymentModel(IOrderService orderService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                ErrorMessage = "Đơn hàng không tồn tại.";
                return Page();
            }

            // Check if order belongs to current user
            var userId = Guid.Parse(userIdStr);
            if (order.CustomerId != userId)
            {
                ErrorMessage = "Bạn không có quyền truy cập đơn hàng này.";
                return Page();
            }

            // Check if order can be paid
            if (order.PaymentStatus == "Paid")
            {
                ErrorMessage = "Đơn hàng này đã được thanh toán.";
                return Page();
            }

            if (order.Status != "Processing")
            {
                ErrorMessage = "Đơn hàng này không thể thanh toán.";
                return Page();
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                ErrorMessage = "Đơn hàng không tồn tại.";
                return Page();
            }

            // Check if order belongs to current user
            var userId = Guid.Parse(userIdStr);
            if (order.CustomerId != userId)
            {
                ErrorMessage = "Bạn không có quyền truy cập đơn hàng này.";
                return Page();
            }

            try
            {
                // Simulate payment processing
                await Task.Delay(1000); // Simulate processing time

                // Update order payment status
                var success = await UpdatePaymentStatusAsync(id, "Paid");
                
                if (success)
                {
                    // Gửi SignalR notification về payment update
                    var updatedOrder = await _orderService.GetOrderByIdAsync(id);
                    if (updatedOrder != null)
                    {
                        await _notificationService.NotifyPaymentUpdated(updatedOrder.OrderNumber, updatedOrder.PaymentStatus, updatedOrder.CustomerName);
                        await _notificationService.NotifyPageReload("orders", "payment_update");
                    }
                    
                    // Chuyển hướng về MyOrders sau khi thanh toán thành công
                    return RedirectToPage("/Orders/MyOrders");
                }
                else
                {
                    ErrorMessage = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại.";
                    Order = order;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
                Order = order;
                return Page();
            }
        }

        private async Task<bool> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return false;

                // Update payment status only
                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTime.Now;

                // Keep order status unchanged - only update payment status

                // Save changes (you'll need to implement this in your OrderService)
                return await _orderService.UpdateOrderAsync(order);
            }
            catch
            {
                return false;
            }
        }
    }
}
