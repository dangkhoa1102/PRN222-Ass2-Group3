using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Vehicles
{
    public class DetailsModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public DetailsModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public Vehicle? Vehicle { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                // Validate ID
                if (id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "ID xe không hợp lệ.";
                    return RedirectToPage("/Vehicles/Index");
                }

                // Debug logging
                Console.WriteLine($"Looking for vehicle with ID: {id}");

                // Load vehicle by ID from database
                Vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                Console.WriteLine($"Vehicle found: {Vehicle != null}");
                
                if (Vehicle == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe với ID này.";
                    return RedirectToPage("/Vehicles/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vehicle: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể tải thông tin xe: " + ex.Message;
                return RedirectToPage("/Vehicles/Index");
            }
        }
    }
}
