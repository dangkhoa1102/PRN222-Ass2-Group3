using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;

        public Order? Order { get; set; }
        public string? CurrentUserRole { get; set; }

        public DetailsModel(IOrderServiceCus orderService)
        {
            _orderService = orderService;
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
                                string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);
            
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
                                        string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);
                    var isCustomer = string.Equals(CurrentUserRole, "Customer", StringComparison.OrdinalIgnoreCase);

                    // Check cancellation permissions
                    if (isCustomer)
                    {
                        // Customer can only cancel if order is unpaid and processing
                        if (order.CustomerId != userId || 
                            order.PaymentStatus != "Unpaid" || 
                            order.Status != "Processing")
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
                            return RedirectToPage("/Orders/MyOrders");
                        }
                        else if (isStaffOrAdmin)
                        {
                            return RedirectToPage("/Admin/ManageOrders");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể hủy đơn hàng.";
                    }
                }
                else if (!string.IsNullOrEmpty(newStatus))
                {
                    // Handle order status update
                    var isStaffOrAdmin = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                        string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);
                    var isCustomer = string.Equals(CurrentUserRole, "Customer", StringComparison.OrdinalIgnoreCase);

                    if (isCustomer && newStatus == "Completed")
                    {
                        // Customer can complete order if it's delivered
                        var order = await _orderService.GetOrderByIdAsync(id);
                        if (order == null || order.Status != "Delivered")
                        {
                            TempData["ErrorMessage"] = "Chỉ có thể hoàn thành đơn hàng đã được giao.";
                            return RedirectToPage(new { id });
                        }
                    }
                    else if (!isStaffOrAdmin)
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền cập nhật trạng thái đơn hàng.";
                        return RedirectToPage(new { id });
                    }

                    var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);
                    
                    if (result)
                    {
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