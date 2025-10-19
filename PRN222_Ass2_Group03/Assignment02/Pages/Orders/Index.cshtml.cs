using Business_Logic_Layer.Interfaces;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;

namespace Assignment02.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IEnumerable<Order> Orders { get; set; } = new List<Order>();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatusFilter { get; set; }   // ✅ thêm filter trạng thái thanh toán

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        // Danh sách trạng thái đơn
        public List<string> StatusList { get; } = new List<string>
        {
            "confirmed",
            "delivering",
            "completed",
            "cancelled"
        };

        // Danh sách trạng thái thanh toán ✅
        public List<string> PaymentStatusList { get; } = new List<string>
        {
            "Unpaid",
            "Paid"
        };

        public async Task OnGetAsync()
        {
            await LoadOrdersAsync();
        }

        // ✅ Thêm tham số PaymentStatusFilter
        public async Task<JsonResult> OnGetSearchAsync(string? searchKeyword, string? statusFilter, string? paymentStatusFilter)
        {
            SearchKeyword = searchKeyword;
            StatusFilter = statusFilter;
            PaymentStatusFilter = paymentStatusFilter;
            await LoadOrdersAsync();

            var result = Orders.Select(o => new
            {
                id = o.Id,
                orderNumber = o.OrderNumber,
                customerName = o.Customer?.FullName ?? "N/A",
                customerPhone = o.Customer?.Phone ?? "",
                vehicleModel = o.Vehicle?.Model ?? "N/A",
                vehicleBrand = o.Vehicle?.Brand ?? "",
                totalAmount = o.TotalAmount,
                status = o.Status,
                paymentStatus = o.PaymentStatus,
                createdAt = o.CreatedAt
            });

            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(id);
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                    ? "Order deleted successfully!"
                    : "Order not found!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToPage(new { SearchKeyword, StatusFilter, PaymentStatusFilter });
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    TempData["ErrorMessage"] = "Please login!";
                    return RedirectToPage("/Login");
                }

                var userId = Guid.Parse(userIdString);
                var success = await _orderService.UpdateOrderStatusAsync(id, status, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Status updated to '{GetStatusDisplay(status)}' successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Order not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToPage(new { SearchKeyword, StatusFilter, PaymentStatusFilter });
        }

        private async Task LoadOrdersAsync()
        {
            var allOrders = await _orderService.GetAllOrdersAsync();

            // 🔍 Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var normalizedKeyword = RemoveDiacritics(SearchKeyword.Trim()).ToLowerInvariant();

                allOrders = allOrders.Where(o =>
                {
                    var orderNumber = RemoveDiacritics(o.OrderNumber ?? "").ToLowerInvariant();
                    var customerName = RemoveDiacritics(o.Customer?.FullName ?? "").ToLowerInvariant();
                    var phone = (o.Customer?.Phone ?? "").ToLowerInvariant();

                    return orderNumber.Contains(normalizedKeyword) ||
                           customerName.Contains(normalizedKeyword) ||
                           phone.Contains(normalizedKeyword);
                });
            }

            // ✅ Lọc theo Status
            if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
            {
                allOrders = allOrders.Where(o =>
                    !string.IsNullOrEmpty(o.Status) &&
                    string.Equals(o.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            // ✅ Lọc theo PaymentStatus
            if (!string.IsNullOrWhiteSpace(PaymentStatusFilter) && PaymentStatusFilter != "All")
            {
                allOrders = allOrders.Where(o =>
                    !string.IsNullOrEmpty(o.PaymentStatus) &&
                    string.Equals(o.PaymentStatus, PaymentStatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            Orders = allOrders.OrderByDescending(o => o.CreatedAt).ToList();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string GetStatusDisplay(string? status)
        {
            return status?.ToLower() switch
            {
                "confirmed" => "Confirmed",
                "delivering" => "Delivering",
                "completed" => "Completed",
                "cancelled" => "Cancelled",
                _ => status ?? "Unknown"
            };
        }

        public static string GetStatusBadgeClass(string? status)
        {
            return status?.ToLower() switch
            {
                "confirmed" => "badge-confirmed",
                "delivering" => "badge-processing",
                "completed" => "badge-completed",
                "cancelled" => "badge-cancelled",
                _ => "badge-secondary"
            };
        }
    }
}
