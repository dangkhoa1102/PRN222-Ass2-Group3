using Business_Logic_Layer.Services;
using Business_Logic_Layer.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Assignment02.Pages.Vehicles
{
    public class EditModel : AuthenticatedPageModel
    {
        private readonly IVehicleService _vehicleService;

        public EditModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [BindProperty]
        public VehicleDTO? Vehicle { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

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
            if (!ModelState.IsValid || Vehicle == null)
            {
                return Page();
            }

            try
            {
                // Get existing vehicle to preserve current image
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(Vehicle.Id);
                if (existingVehicle == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy xe để cập nhật.");
                    return Page();
                }

                // Handle image upload if new image is provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "vehicles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var fileName = $"{Vehicle.Id}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Update image path
                    Vehicle.Images = $"/images/vehicles/{fileName}";
                }
                else
                {
                    // Keep existing image if no new image uploaded
                    Vehicle.Images = existingVehicle.Images;
                }

                // Update vehicle in database
                await _vehicleService.UpdateVehicleAsync(Vehicle);
                
                // Redirect to vehicle details page instead of list
                return RedirectToPage("/Vehicles/Details", new { id = Vehicle.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật xe: " + ex.Message);
                return Page();
            }
        }
    }
}
