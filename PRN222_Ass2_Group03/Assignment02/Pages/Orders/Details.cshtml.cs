using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class DetailsModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public OrderDTO? Order { get; set; }

        public DetailsModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Authorization: allow Admin/Staff or the owner (CustomerId matches session UserId)
            var isPrivileged = string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase);
            if (!isPrivileged)
            {
                if (Guid.TryParse(UserId, out var currentUserId))
                {
                    if (order.CustomerId != currentUserId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return RedirectToPage("/Login");
                }
            }
            Order = order;
            return Page();
        }
    }
}
