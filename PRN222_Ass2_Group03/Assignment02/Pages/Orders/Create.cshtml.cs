using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Assignment02.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IOrderService _orderService;

        public CreateModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty]
        public OrderInputModel Input { get; set; } = new();

        public IEnumerable<Vehicle> AvailableVehicles { get; set; } = new List<Vehicle>();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                Input.PaymentStatus = "Unpaid";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
                return RedirectToPage("/Orders/OrderList");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // If existing customer selected
                if (Input.CustomerId.HasValue && Input.CustomerId.Value != Guid.Empty)
                {
                    ModelState.Remove("Input.CustomerName");
                    ModelState.Remove("Input.CustomerPhone");
                    ModelState.Remove("Input.CustomerEmail");
                }
                else
                {
                    // Validate new customer information
                    if (string.IsNullOrWhiteSpace(Input.CustomerName))
                    {
                        ModelState.AddModelError("Input.CustomerName", "Customer name is required");
                    }
                    else if (Input.CustomerName.Trim().Length < 2 || Input.CustomerName.Trim().Length > 100)
                    {
                        ModelState.AddModelError("Input.CustomerName", "Name must be between 2-100 characters");
                    }

                    if (string.IsNullOrWhiteSpace(Input.CustomerPhone))
                    {
                        ModelState.AddModelError("Input.CustomerPhone", "Phone number is required");
                    }
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(Input.CustomerPhone.Trim(), @"^0\d{9,10}$"))
                    {
                        ModelState.AddModelError("Input.CustomerPhone", "Phone must start with 0 and contain 10-11 digits");
                    }

                    if (!string.IsNullOrWhiteSpace(Input.CustomerEmail) &&
                        !System.Text.RegularExpressions.Regex.IsMatch(Input.CustomerEmail.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Input.CustomerEmail", "Invalid email format");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new {
                            Key = x.Key,
                            Errors = string.Join("; ", x.Value.Errors.Select(e => e.ErrorMessage))
                        })
                        .ToList();

                    var errorDetails = string.Join(" | ", errors.Select(e => $"{e.Key}: {e.Errors}"));

                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    ErrorMessage = $"Validation failed: {errorDetails}";
                    return Page();
                }

                // Handle customer information
                User? customer = await HandleCustomerInformationAsync();
                if (customer == null)
                {
                    ErrorMessage = "Unable to process customer information. Please try again.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // Validate vehicle
                var vehicle = await _orderService.GetVehicleByIdAsync(Input.VehicleId);
                if (vehicle == null)
                {
                    ErrorMessage = "Vehicle not found. Please select another vehicle.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                if (vehicle.StockQuantity <= 0)
                {
                    ErrorMessage = $"{vehicle.Brand} {vehicle.Model} is out of stock. Please select another vehicle.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                // Validate total amount
                if (Input.TotalAmount != vehicle.Price)
                {
                    ErrorMessage = "Total amount does not match vehicle price. Please try again.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                var dealerId = GetCurrentDealerId();

                // Get default dealer if needed
                if (dealerId == Guid.Empty)
                {
                    var firstDealer = await _orderService.GetFirstDealerAsync();
                    if (firstDealer == null)
                    {
                        ErrorMessage = "No dealer found in system. Please contact administrator.";
                        AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                        return Page();
                    }
                    dealerId = firstDealer.Id;
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = _orderService.GenerateOrderNumber(),
                    CustomerId = customer.Id,
                    VehicleId = Input.VehicleId,
                    DealerId = dealerId,
                    TotalAmount = Input.TotalAmount,
                    Status = "confirmed",
                    PaymentStatus = Input.PaymentStatus,
                    Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
                    CreatedAt = DateTime.Now
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);

                if (createdOrder == null)
                {
                    ErrorMessage = "An error occurred while creating the order. Please try again.";
                    AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                    return Page();
                }

                SuccessMessage = $"✓ Order {createdOrder.OrderNumber} created successfully!";
                return RedirectToPage("/Orders/OrderList");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"System error: {ex.Message}";
                AvailableVehicles = await _orderService.GetAvailableVehiclesAsync();
                return Page();
            }
        }

        private async Task<User?> HandleCustomerInformationAsync()
        {
            try
            {
                // CASE 1: Existing customer selected from dropdown
                if (Input.CustomerId.HasValue && Input.CustomerId.Value != Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"Finding customer with ID: {Input.CustomerId.Value}");

                    var existingCustomer = await _orderService.GetCustomerByIdAsync(Input.CustomerId.Value);

                    if (existingCustomer != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✓ Customer found: {existingCustomer.FullName}");
                        return existingCustomer;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✗ Customer NOT found in DB!");
                        ModelState.AddModelError("", $"Customer not found with ID: {Input.CustomerId.Value}");
                        return null;
                    }
                }

                // CASE 2: Create new customer with provided information
                if (string.IsNullOrWhiteSpace(Input.CustomerName) ||
                    string.IsNullOrWhiteSpace(Input.CustomerPhone))
                {
                    ModelState.AddModelError("", "Please select an existing customer OR enter new customer information (Name and Phone are required).");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Creating new customer: {Input.CustomerName} - {Input.CustomerPhone}");

                var newCustomer = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = Input.CustomerName.Trim(),
                    Phone = Input.CustomerPhone.Trim(),
                    Email = string.IsNullOrWhiteSpace(Input.CustomerEmail) ? null : Input.CustomerEmail.Trim(),
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var createdCustomer = await _orderService.CreateCustomerAsync(newCustomer);

                if (createdCustomer != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ New customer created successfully: {createdCustomer.Id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✗ Failed to create new customer!");
                }

                return createdCustomer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Customer processing error: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> OnGetSearchCustomersAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return new JsonResult(new { results = Array.Empty<object>() });
                }

                var customers = await _orderService.SearchCustomersAsync(term.Trim());

                var results = customers.Select(c => new
                {
                    id = c.Id,
                    text = $"{c.FullName} - {c.Phone}",
                    fullName = c.FullName,
                    phone = c.Phone,
                    email = c.Email ?? ""
                }).ToList();

                return new JsonResult(new { results });
            }
            catch (Exception)
            {
                return new JsonResult(new { results = Array.Empty<object>() });
            }
        }

        public async Task<IActionResult> OnGetVehicleInfoAsync(Guid vehicleId)
        {
            try
            {
                var vehicle = await _orderService.GetVehicleByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound(new { error = "Vehicle not found" });
                }

                return new JsonResult(new
                {
                    id = vehicle.Id,
                    name = $"{vehicle.Brand} {vehicle.Model}",
                    price = vehicle.Price,
                    stock = vehicle.StockQuantity,
                    specifications = vehicle.Specifications
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private Guid GetCurrentDealerId()
        {
            var dealerId = HttpContext.Session.GetString("DealerId");
            if (!string.IsNullOrEmpty(dealerId) && Guid.TryParse(dealerId, out Guid id))
            {
                return id;
            }

            return Guid.Empty;
        }

        public class OrderInputModel
        {
            public Guid? CustomerId { get; set; }

            [Display(Name = "Customer Name")]
            public string? CustomerName { get; set; }

            [Display(Name = "Phone Number")]
            public string? CustomerPhone { get; set; }

            [Display(Name = "Email")]
            public string? CustomerEmail { get; set; }

            [Required(ErrorMessage = "Please select a vehicle")]
            [Display(Name = "Vehicle")]
            public Guid VehicleId { get; set; }

            [Required(ErrorMessage = "Total amount is required")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
            [Display(Name = "Total Amount")]
            public decimal TotalAmount { get; set; }

            [Required(ErrorMessage = "Please select payment status")]
            [Display(Name = "Payment Status")]
            public string PaymentStatus { get; set; } = "Unpaid";

            [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
            [Display(Name = "Notes")]
            public string? Notes { get; set; }
        }
    }
}