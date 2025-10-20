using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace Assignment02.Pages.Vehicles
{
    public class AddModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public AddModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [BindProperty]
        public Vehicle Vehicle { get; set; } = new Vehicle();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public Task OnGetAsync()
        {
            // No need to load dealers for vehicle creation
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "vehicles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Set image path
                    Vehicle.Images = $"/images/vehicles/{fileName}";
                }
                else
                {
                    // Set default image if no image uploaded
                    Vehicle.Images = "/images/vehicles/default-car.svg";
                }

                // Create vehicle in database
                await _vehicleService.CreateVehicleAsync(Vehicle);
                
                // Redirect to vehicle details page
                return RedirectToPage("/Vehicles/Details", new { id = Vehicle.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm xe: " + ex.Message);
                return Page();
            }
        }
    }
}
