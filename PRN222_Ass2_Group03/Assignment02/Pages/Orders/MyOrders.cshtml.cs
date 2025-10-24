using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class MyOrdersModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly RealTimeNotificationService _notificationService;

        public List<OrderDTO> Orders { get; set; } = new();
        public string? CurrentUserId { get; set; }

        public MyOrdersModel(IOrderServiceCus orderService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            Guid userId = Guid.Parse(userIdStr);
            CurrentUserId = userIdStr;
            
            // Chỉ lấy các order chưa hoàn thành (Processing, Pending, Confirmed, Shipped, Complete)
            // "Complete" orders sẽ được hiển thị để customer có thể chuyển thành DONE
            // "DONE", "Completed", "Cancelled" orders sẽ không hiển thị ở đây (sẽ hiển thị trong Order History)
            var allOrders = await _orderService.GetOrdersByUserIdAsync(userId);
            Orders = allOrders.Where(o => 
                o.Status != "Completed" && 
                o.Status != "Cancelled" && 
                o.Status != "Done" &&
                o.Status != "DONE"
            ).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostMarkDoneAsync(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            Guid userId = Guid.Parse(userIdStr);
            var success = await _orderService.MarkDoneByCustomerAsync(id, userId);
            
            if (success)
            {
                // Get order details for notification
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order != null)
                {
                    // Send SignalR notification to staff
                    await _notificationService.NotifyOrderMarkedDone(order.OrderNumber, order.CustomerName, order.VehicleName);
                    await _notificationService.NotifyPageReload("orders", "order_marked_done");
                }
                
                TempData["SuccessMessage"] = "Order marked as done successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to mark order as done.";
            }
            
            return RedirectToPage();
        }
    }
}