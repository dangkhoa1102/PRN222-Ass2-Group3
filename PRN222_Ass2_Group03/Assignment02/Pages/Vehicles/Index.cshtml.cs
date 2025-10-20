using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.Vehicles
{
    public class IndexModel : PageModel
    {
        private readonly IVehicleService _vehicleService;

        public IndexModel(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public IEnumerable<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public async Task OnGetAsync(string? success)
        {
            // Handle success messages based on query parameter
            if (!string.IsNullOrEmpty(success))
            {
                switch (success.ToLower())
                {
                    case "added":
                        TempData["SuccessMessage"] = "Xe đã được thêm thành công!";
                        break;
                    case "updated":
                        TempData["SuccessMessage"] = "Xe đã được cập nhật thành công!";
                        break;
                    case "deleted":
                        TempData["SuccessMessage"] = "Xe đã được xóa thành công!";
                        break;
                }
            }
            
            // Get all vehicles from database
            Vehicles = await _vehicleService.GetAllVehiclesAsync();
        }

    }
}
