using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment02.Services;

namespace Assignment02.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;
        private readonly RealTimeNotificationService _notificationService;

        public OrderDTO? Order { get; set; }
        public string? CurrentUserRole { get; set; }

        public DetailsModel(IOrderServiceCus orderService, RealTimeNotificationService notificationService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Console.WriteLine($"Orders/Details OnGetAsync called with ID: {id}");
            
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                Console.WriteLine("No user ID in session, redirecting to login");
                return RedirectToPage("/Login");
            }

            CurrentUserRole = HttpContext.Session.GetString("Role");
            Console.WriteLine($"Current user role: {CurrentUserRole}");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                Console.WriteLine($"Order not found with ID: {id}");
                return NotFound();
            }
            
            Console.WriteLine($"Order found: {order.OrderNumber}");

            // Check access permissions
            var userId = Guid.Parse(userIdStr);
            var isStaffOrAdmin = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(CurrentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(CurrentUserRole, "STAFF", StringComparison.OrdinalIgnoreCase);
            
            // Staff/Admin can view all orders, Customer can only view their own orders
            if (!isStaffOrAdmin && order.CustomerId != userId)
            {
                return Forbid();
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, string? newStatus = null, string? cancelReason = null)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToPage("/Login");

            CurrentUserRole = HttpContext.Session.GetString("Role");

            try
            {
                if (!string.IsNullOrEmpty(cancelReason))
                {
                    // Handle order cancellation
                    var order = await _orderService.GetOrderByIdAsync(id);
                    if (order == null)
                    {
                        TempData["ErrorMessage"] = "Order not found.";
                        return RedirectToPage(new { id });
                    }

                    var userId = Guid.Parse(userIdStr);
                    var isStaffOrAdmin = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                        string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(CurrentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(CurrentUserRole, "STAFF", StringComparison.OrdinalIgnoreCase);
                    var isCustomer = string.Equals(CurrentUserRole, "Customer", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(CurrentUserRole, "CUSTOMER", StringComparison.OrdinalIgnoreCase);
                    
                    Console.WriteLine($"Cancel Order - UserId: {userId}, Role: {CurrentUserRole}, isCustomer: {isCustomer}, isStaffOrAdmin: {isStaffOrAdmin}");

                    // Check cancellation permissions
                    if (isCustomer)
                    {
                        // Customer can only cancel if order is unpaid and processing
                        if (order.CustomerId != userId || 
                            !string.Equals(order.PaymentStatus, "Unpaid", StringComparison.OrdinalIgnoreCase) || 
                            !string.Equals(order.Status, "Processing", StringComparison.OrdinalIgnoreCase))
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng này.";
                            return RedirectToPage(new { id });
                        }
                    }
                    else if (!isStaffOrAdmin)
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng.";
                        return RedirectToPage(new { id });
                    }

                    // Cancel the order
                    var result = await _orderService.CancelOrderAsync(id, cancelReason);
                    
                    if (result)
                    {
                        TempData["SuccessMessage"] = $"Đơn hàng đã được hủy với lý do: {cancelReason}";
                        
                        // Redirect based on user role after cancellation
                        if (isCustomer)
                        {
                            Console.WriteLine("Redirecting customer to MyOrders");
                            return RedirectToPage("/Orders/MyOrders");
                        }
                        else if (isStaffOrAdmin)
                        {
                            Console.WriteLine("Redirecting staff/admin to ManageOrders");
                            return RedirectToPage("/Admin/ManageOrders");
                        }
                        else
                        {
                            Console.WriteLine("Fallback: redirecting back to order details");
                            return RedirectToPage(new { id });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể hủy đơn hàng.";
                        return RedirectToPage(new { id });
                    }
                }
                else if (!string.IsNullOrEmpty(newStatus))
                {
                    // Handle order status update
                    var isStaffOrAdmin = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                        string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(CurrentUserRole, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(CurrentUserRole, "STAFF", StringComparison.OrdinalIgnoreCase);
                    var isCustomer = string.Equals(CurrentUserRole, "Customer", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(CurrentUserRole, "CUSTOMER", StringComparison.OrdinalIgnoreCase);

                    if (isCustomer && (newStatus == "Completed" || newStatus == "DONE"))
                    {
                        // Customer can complete order if it's delivered, or mark as DONE if it's Complete
                        var order = await _orderService.GetOrderByIdAsync(id);
                        if (order == null)
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                            return RedirectToPage(new { id });
                        }
                        
                        if (newStatus == "Completed" && order.Status != "Delivered")
                        {
                            TempData["ErrorMessage"] = "Chỉ có thể hoàn thành đơn hàng đã được giao.";
                            return RedirectToPage(new { id });
                        }
                        
                        if (newStatus == "DONE" && order.Status != "Complete")
                        {
                            TempData["ErrorMessage"] = "Chỉ có thể đánh dấu DONE cho đơn hàng đã hoàn thành.";
                            return RedirectToPage(new { id });
                        }
                    }
                    else if (!isStaffOrAdmin && !isCustomer)
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền cập nhật trạng thái đơn hàng.";
                        return RedirectToPage(new { id });
                    }

                    // Check if trying to ship an unpaid order
                    if (newStatus == "Shipped")
                    {
                        var order = await _orderService.GetOrderByIdAsync(id);
                        if (order != null && string.Equals(order.PaymentStatus, "Unpaid", StringComparison.OrdinalIgnoreCase))
                        {
                            TempData["ErrorMessage"] = "Đơn hàng chưa thanh toán, không thể giao hàng!";
                            return RedirectToPage(new { id });
                        }
                    }

                    var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);
                    
                    if (result)
                    {
                        // Get order details for notification
                        var orderForNotification = await _orderService.GetOrderByIdAsync(id);
                        if (orderForNotification != null)
                        {
                            // Send SignalR notification
                            await _notificationService.NotifyOrderUpdated(orderForNotification.OrderNumber, newStatus);
                            await _notificationService.NotifyPageReload("orders", "status_update");
                        }
                        
                        TempData["SuccessMessage"] = $"Trạng thái đơn hàng đã được cập nhật thành {newStatus}!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToPage(new { id });
        }
    }
}