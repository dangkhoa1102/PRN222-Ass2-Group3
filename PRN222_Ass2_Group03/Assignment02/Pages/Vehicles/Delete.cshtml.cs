using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Vehicles
{
    public class DeleteModel : AuthenticatedPageModel
    {
        private readonly IVehicleService _vehicleService;

        public DeleteModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [BindProperty]
        public Vehicle? Vehicle { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (!IsAuthenticated || (!string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToPage("/Login");
            }
            try
            {
                // Validate ID
                if (id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "ID xe không hợp lệ.";
                    return RedirectToPage("/Vehicles/Index");
                }

                // Load vehicle by ID from database
                Vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (Vehicle == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe với ID này.";
                    return RedirectToPage("/Vehicles/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể tải thông tin xe: " + ex.Message;
                return RedirectToPage("/Vehicles/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsAuthenticated || (!string.Equals(CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(CurrentUserRole, "Staff", StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToPage("/Login");
            }
            try
            {
                if (Vehicle == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe để xóa.";
                    return RedirectToPage("/Vehicles/Index");
                }

                // Delete vehicle from database
                bool deleted = await _vehicleService.DeleteVehicleAsync(Vehicle.Id);
                
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Không thể xóa xe này.";
                    return RedirectToPage("/Vehicles/Index");
                }
                
                // Redirect to vehicle list page
                return RedirectToPage("/Vehicles/Index", new { success = "deleted" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa xe: " + ex.Message;
                return RedirectToPage("/Vehicles/Index");
            }
        }
    }
}
