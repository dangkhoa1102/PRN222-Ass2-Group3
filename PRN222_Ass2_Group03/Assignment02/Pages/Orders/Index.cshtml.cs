using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class IndexModel : AuthenticatedPageModel
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

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                Response.Redirect("/Login");
                return;
            }

            var userId = Guid.Parse(UserId!);
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);

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
