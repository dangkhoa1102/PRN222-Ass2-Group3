using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class ViewOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PaymentStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public ViewOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
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

            Orders = orders;
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
                bool result = await _orderService.CancelOrderAsync(id, "Cancelled by user");

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
