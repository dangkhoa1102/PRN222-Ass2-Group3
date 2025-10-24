using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment02.Pages.TestDrive
{
    public class IndexModel : PageModel
    {
        private readonly ITestDriveService _testDriveService;
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ITestDriveService testDriveService, ILogger<IndexModel> logger)
        {
            _testDriveService = testDriveService;
            _logger = logger;
        }
        public List<TestDriveAppointment> TestDrives { get; set; } 
        public async Task<IActionResult> OnPostConfirmAsync(Guid id)
        {
            var testDrive = await _testDriveService.GetTestDriveByIdAsync(id);
            if(testDrive != null)
            {
                testDrive.Status = "Confirmed";
                await _testDriveService.ConfirmTestDriveAsync(id);
            }
            return RedirectToPage(); 
        }
        public async Task OnGetAsync()
        {
            try{
                var userId = GetCurrentUserId();
                var role = GetUserRole();
                _logger.LogInformation($"User ID: {userId}, Role: {role}");
                if (role == "admin" || role == "dealer_staff")
                {
                    TestDrives = await _testDriveService.GetAllTestDrivesAsync();
                }
                else
                {
                    // customer 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching test drives: {ex.Message}");
                TestDrives = new List<TestDriveAppointment>();
            }
        }
        public Guid GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            throw new Exception("User is not logged in.");
        }
        public string GetUserRole()
        {
            return HttpContext.Session.GetString("Role") ?? "Customer";
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id, string note)
        {
            await _testDriveService.CancelTestDriveAsync(id, note);
            return RedirectToPage();
        }
    }
}
