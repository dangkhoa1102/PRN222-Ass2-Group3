using Assignment02.Hubs;
using Assignment02.Services;
using Business_Logic_Layer.Services;

// Add Razor Pages & SignalR
var builder = WebApplication.CreateBuilder(args);

// ===============================
// Add EF DbContext
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Register Service Factory
builder.Services.AddScoped<ServiceFactory>();

// Register Services through Factory
builder.Services.AddScoped<IUserService>(provider => provider.GetRequiredService<ServiceFactory>().CreateUserService());
builder.Services.AddScoped<IOrderServiceCus>(provider => provider.GetRequiredService<ServiceFactory>().CreateOrderService());
builder.Services.AddScoped<ICustomerTestDriveAppointmentService>(provider => provider.GetRequiredService<ServiceFactory>().CreateCustomerTestDriveAppointmentService());
builder.Services.AddScoped<IVehicleService>(provider => provider.GetRequiredService<ServiceFactory>().CreateVehicleService());
builder.Services.AddScoped<IDealerService>(provider => provider.GetRequiredService<ServiceFactory>().CreateDealerService());

// Register SignalR Services
builder.Services.AddScoped<RealTimeNotificationService>();

// ===============================
// 💾 Session Configuration
// ===============================

// ===============================
// 💾 Session Configuration
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
});

// ===============================
// 🔐 Authentication & Authorization
// ===============================
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
    });

builder.Services.AddAuthorization();
// ===============================
// ⚙️ Middleware Configuration
// ===============================
// ===============================
var app = builder.Build();

// ===============================
// ⚙️ Middleware Configuration
// ===============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseSession();

// ✅ Redirect unauthenticated users
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        var userId = context.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.Redirect("/Login");
            return;
        }
        else
        {
            context.Response.Redirect("/Index");
            return;
        }
    }
    await next();
});

app.UseAuthorization();

// ===============================
// 🌐 Map Razor Pages & SignalR Hubs
// ===============================
app.MapRazorPages();
app.MapHub<RealTimeHub>("/realtimehub");

// Test database connection on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var serviceFactory = scope.ServiceProvider.GetRequiredService<ServiceFactory>();
        var orderService = serviceFactory.CreateOrderService();
        var orders = await orderService.GetAllOrdersAsync();
        Console.WriteLine($"Database connection successful. Found {orders.Count} orders.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

app.Run();
