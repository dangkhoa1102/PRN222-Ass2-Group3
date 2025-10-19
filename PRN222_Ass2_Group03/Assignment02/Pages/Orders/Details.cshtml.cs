using Business_Logic_Layer.Interfaces;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }

        // Helper ?? hi?n th? status
        public static string GetStatusDisplay(string? status)
        {
            return status?.ToLower() switch
            {
                "processing" => "?ang x? lý",
                "confirmed" => "?ã xác nh?n",
                "shipping" => "?ang giao hàng",
                "completed" => "Hoàn thành",
                "cancelled" => "?ã h?y",
                _ => status ?? "Không xác ??nh"
            };
        }

        public static string GetStatusBadgeClass(string? status)
        {
            return status?.ToLower() switch
            {
                "processing" => "badge bg-info",
                "confirmed" => "badge bg-primary",
                "shipping" => "badge bg-warning",
                "completed" => "badge bg-success",
                "cancelled" => "badge bg-danger",
                _ => "badge bg-secondary"
            };
        }

        public static string GetPaymentBadgeClass(string? payment)
        {
            return payment?.ToLower() switch
            {
                "paid" => "badge bg-success",
                "unpaid" => "badge bg-danger",
                "partial" => "badge bg-warning",
                _ => "badge bg-secondary"
            };
        }
    }
}