using Business_Logic_Layer.DTOs;
using Business_Logic_Layer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Orders
{
    public class PendingOrdersModel : AuthenticatedPageModel
    {
        private readonly IOrderService _orderService;

        public List<OrderDTO> PendingOrders { get; set; } = new();

        public PendingOrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            Guid userId = Guid.Parse(UserId!);
            PendingOrders = await _orderService.GetPendingOrdersByUserIdAsync(userId);
            return Page();
        }

        // ? X? l� khi ng??i d�ng nh?n n�t Cancel
        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            if (!IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // G?i h�m CancelOrderAsync trong Service
                bool result = await _orderService.CancelOrderAsync(id, "Cancelled by user");

                if (result)
                {
                    TempData["SuccessMessage"] = "??n h�ng ?� ???c h?y th�nh c�ng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Kh�ng th? h?y ??n h�ng n�y.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"?� x?y ra l?i: {ex.Message}";
            }

            // Redirect l?i trang hi?n t?i ?? c?p nh?t danh s�ch
            return RedirectToPage();
        }
    }
}
