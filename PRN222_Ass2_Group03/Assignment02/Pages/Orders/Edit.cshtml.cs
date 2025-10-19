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
        // private readonly IVehicleService _vehicleService; // Chờ bạn khác làm xong

        public EditModel(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
            // _vehicleService = vehicleService;
        }

        [BindProperty]
        public Order Order { get; set; } = null!;

        public SelectList Customers { get; set; } = null!;
        public SelectList Vehicles { get; set; } = null!;
        public SelectList StatusList { get; set; } = null!;

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
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập!";
                    return RedirectToPage("/Login");
                }

                // Lấy order cũ từ database
                var existingOrder = await _orderService.GetOrderByIdAsync(Order.Id);
                if (existingOrder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToPage("./Index");
                }

                // Chỉ cập nhật những trường được thay đổi (không null/empty)
                if (Order.CustomerId != Guid.Empty && Order.CustomerId != existingOrder.CustomerId)
                {
                    existingOrder.CustomerId = Order.CustomerId;
                }

                // VehicleId: Chỉ cập nhật nếu có chọn xe mới (khác Empty và khác giá trị cũ)
                // Nếu không chọn (Empty) thì giữ nguyên xe cũ
                if (Order.VehicleId != Guid.Empty && Order.VehicleId != existingOrder.VehicleId)
                {
                    existingOrder.VehicleId = Order.VehicleId;
                }

                if (!string.IsNullOrWhiteSpace(Order.Status))
                {
                    existingOrder.Status = Order.Status;
                }

                if (Order.TotalAmount > 0)
                {
                    existingOrder.TotalAmount = Order.TotalAmount;
                }

                if (Order.CreatedAt != default(DateTime))
                {
                    existingOrder.CreatedAt = Order.CreatedAt;
                }

                if (!string.IsNullOrWhiteSpace(Order.Notes))
                {
                    existingOrder.Notes = Order.Notes;
                }

                // Cập nhật thời gian sửa
                existingOrder.UpdatedAt = DateTime.Now;

                var success = await _orderService.UpdateOrderAsync(existingOrder);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = "Không thể cập nhật đơn hàng!";
                    await LoadSelectListsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
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

            // TODO: Đợi bạn làm VehicleService xong rồi bỏ comment
            // var vehicles = await _vehicleService.GetAllVehiclesAsync();
            // var vehicleList = vehicles
            //     .Select(v => new { 
            //         v.Id, 
            //         DisplayName = $"{v.Make} {v.Model} - {v.Year}" 
            //     })
            //     .ToList();
            // Vehicles = new SelectList(vehicleList, "Id", "DisplayName", Order?.VehicleId);

            // TẠM THỜI dùng list rỗng để không bị lỗi
            Vehicles = new SelectList(new List<SelectListItem>(), "Value", "Text");

            // Danh sách trạng thái
            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "processing", Text = "Đang xử lý" },
                new SelectListItem { Value = "confirmed", Text = "Đã xác nhận" },
                new SelectListItem { Value = "shipping", Text = "Đang giao hàng" },
                new SelectListItem { Value = "completed", Text = "Hoàn thành" },
                new SelectListItem { Value = "cancelled", Text = "Đã hủy" }
            };

            StatusList = new SelectList(statuses, "Value", "Text", Order?.Status?.ToLower());
        }
    }
}