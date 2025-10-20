using Business_Logic_Layer.Interfaces;
using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Assignment02.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public EditModel(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        [BindProperty]
        public Order Order { get; set; } = null!;

        public SelectList Customers { get; set; } = null!;
        public SelectList Vehicles { get; set; } = null!;
        public SelectList StatusList { get; set; } = null!;

        public SelectList PaymentStatusList { get; set; } = null!;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

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
            await LoadSelectListsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    TempData["ErrorMessage"] = "Please login!";
                    return RedirectToPage("/Login");
                }

                // Lấy order cũ từ database
                var existingOrder = await _orderService.GetOrderByIdAsync(Order.Id);
                if (existingOrder == null)
                {
                    TempData["ErrorMessage"] = "Order not found!";
                    return RedirectToPage("./Index");
                }

                // ✅ Cập nhật CustomerId
                if (Order.CustomerId != Guid.Empty && Order.CustomerId != existingOrder.CustomerId)
                {
                    existingOrder.CustomerId = Order.CustomerId;
                }

                // ✅ Cập nhật VehicleId
                if (Order.VehicleId != Guid.Empty && Order.VehicleId != existingOrder.VehicleId)
                {
                    existingOrder.VehicleId = Order.VehicleId;
                }

                // ✅ Cập nhật Status (trạng thái đơn hàng)
                if (!string.IsNullOrWhiteSpace(Order.Status))
                {
                    existingOrder.Status = Order.Status;
                }

                // ✅ THÊM: Cập nhật PaymentStatus (trạng thái thanh toán)
                if (!string.IsNullOrWhiteSpace(Order.PaymentStatus))
                {
                    existingOrder.PaymentStatus = Order.PaymentStatus;
                }

                // ✅ Cập nhật TotalAmount
                if (Order.TotalAmount > 0)
                {
                    existingOrder.TotalAmount = Order.TotalAmount;
                }

                // ✅ Cập nhật CreatedAt (nếu cần)
                if (Order.CreatedAt != default(DateTime))
                {
                    existingOrder.CreatedAt = Order.CreatedAt;
                }

                // ✅ Cập nhật Notes
                if (!string.IsNullOrWhiteSpace(Order.Notes))
                {
                    existingOrder.Notes = Order.Notes;
                }
                else
                {
                    // Nếu Notes bị xóa trống, cho phép cập nhật thành null
                    existingOrder.Notes = null;
                }

                // Cập nhật thời gian sửa
                existingOrder.UpdatedAt = DateTime.Now;

                var success = await _orderService.UpdateOrderAsync(existingOrder);

                if (success)
                {
                    TempData["SuccessMessage"] = "Order updated successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = "Failed to update order!";
                    await LoadSelectListsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await LoadSelectListsAsync();
                return Page();
            }
        }

        private async Task LoadSelectListsAsync()
        {
            // Lấy danh sách customers
            var customers = await _userService.GetAllUsersAsync();
            var customerList = customers
                .Where(u => u.Role?.ToLower() == "customer")
                .Select(u => new { u.Id, u.FullName })
                .ToList();

            Customers = new SelectList(customerList, "Id", "FullName", Order?.CustomerId);

            // TODO: Lấy danh sách vehicles
            // var vehicles = await _vehicleService.GetAllVehiclesAsync();
            // Vehicles = new SelectList(vehicles, "Id", "DisplayName", Order?.VehicleId);

            // TẠM THỜI dùng list rỗng
            Vehicles = new SelectList(new List<SelectListItem>(), "Value", "Text");

            // ✅ Danh sách Order Status (trạng thái đơn hàng)
            var statuses = new List<SelectListItem>
            {
               
                new SelectListItem { Value = "confirmed", Text = "Confirmed" },
                new SelectListItem { Value = "shipping", Text = "Shipping" },
                new SelectListItem { Value = "completed", Text = "Completed" },
                new SelectListItem { Value = "cancelled", Text = "Cancelled" }
            };
            StatusList = new SelectList(statuses, "Value", "Text", Order?.Status?.ToLower());

            // ✅ THÊM: Danh sách Payment Status (trạng thái thanh toán)
            var paymentStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Unpaid", Text = "Unpaid" },
                new SelectListItem { Value = "Paid", Text = "Paid" },
               
            };
            PaymentStatusList = new SelectList(paymentStatuses, "Value", "Text", Order?.PaymentStatus);
        }
    }
}