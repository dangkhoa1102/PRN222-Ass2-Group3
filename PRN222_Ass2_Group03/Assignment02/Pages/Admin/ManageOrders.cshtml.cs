using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Admin
{
    public class ManageOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public ManageOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
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
                // Update order status (you might need to implement UpdateOrderStatusAsync in IOrderService)
                var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                
                if (result)
                {
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

