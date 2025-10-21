using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderServiceCus _orderService;

        public List<OrderDTO> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public string? PaymentStatus { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }

        public IndexModel(IOrderServiceCus orderService)
        {
            _orderService = orderService;
        }

        public async Task OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                Response.Redirect("/Login");
                return;
            }

            var userId = Guid.Parse(userIdStr);
            var orders = (await _orderService.GetOrdersByUserIdAsync(userId)).ToList();

            if (!string.IsNullOrEmpty(Status))
                orders = orders.Where(o => o.Status == Status).ToList();

            if (!string.IsNullOrEmpty(PaymentStatus))
                orders = orders.Where(o => o.PaymentStatus == PaymentStatus).ToList();

            if (FromDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= FromDate).ToList();

            if (ToDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= ToDate).ToList();

            Orders = orders;
        }
    }
}