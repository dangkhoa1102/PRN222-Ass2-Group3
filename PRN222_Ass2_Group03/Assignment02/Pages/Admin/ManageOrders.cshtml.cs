using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Assignment02.Services;

namespace Assignment02.Pages.Admin
{
    public class ManageOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly RealTimeNotificationService _notificationService;

        public ManageOrdersModel(IOrderServiceCus orderService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public List<OrderDTO> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated || (!string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToPage("/Login");
            }

            // Get all orders as DTOs
            var allOrders = await _orderService.GetAllOrdersDTOAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(Status))
                allOrders = allOrders.Where(o => o.Status == Status).ToList();

            if (!string.IsNullOrEmpty(PaymentStatus))
                allOrders = allOrders.Where(o => o.PaymentStatus == PaymentStatus).ToList();

            if (FromDate.HasValue)
                allOrders = allOrders.Where(o => o.CreatedAt >= FromDate).ToList();

            if (ToDate.HasValue)
                allOrders = allOrders.Where(o => o.CreatedAt <= ToDate).ToList();

            Orders = allOrders;
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid orderId, string newStatus)
        {
            if (!IsAuthenticated || (!string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Check if trying to ship an unpaid order
                if (newStatus == "Shipped")
                {
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order != null && string.Equals(order.PaymentStatus, "Unpaid", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["ErrorMessage"] = "Đơn hàng chưa thanh toán, không thể giao hàng!";
                        return RedirectToPage();
                    }
                }

                // Update order status
                var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                
                if (result)
                {
                    // Get order details for notification
                    var order = await _orderService.GetOrderByIdAsync(orderId);
                    if (order != null)
                    {
                        // Send SignalR notification
                        await _notificationService.NotifyOrderUpdated(order.OrderNumber, newStatus);
                        await _notificationService.NotifyPageReload("orders", "status_update");
                    }
                    
                    TempData["SuccessMessage"] = "Order status updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update order status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating order status: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}

