using Assignment02.Hubs;
using Business_Logic_Layer.Services;
<<<<<<<<< Temporary merge branch 1
using DataAccess_Layer.Repositories.Implement;
using DataAccess_Layer.Repositories.Interface;
=========
>>>>>>>>> Temporary merge branch 2
// Add Razor Pages & SignalR
var builder = WebApplication.CreateBuilder(args);

// ===============================
// Add EF DbContext
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Register Service Factory
builder.Services.AddScoped<ServiceFactory>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Register Services through Factory
builder.Services.AddScoped<IUserService>(provider => provider.GetRequiredService<ServiceFactory>().CreateUserService());
builder.Services.AddScoped<IOrderService>(provider => provider.GetRequiredService<ServiceFactory>().CreateOrderService());
builder.Services.AddScoped<ICustomerTestDriveAppointmentService>(provider => provider.GetRequiredService<ServiceFactory>().CreateCustomerTestDriveAppointmentService());
builder.Services.AddScoped<IVehicleService>(provider => provider.GetRequiredService<ServiceFactory>().CreateVehicleService());
// Register repositories and services (they will use EVDealerSystemContext's own connection)
// ===============================
// 💾 Session Configuration
// ===============================
builder.Services.AddScoped<ICustomerTestDriveAppointmentService, CustomerTestDriveAppointmentService>();
>>>>>>>>> Temporary merge branch 2

// ===============================
// 💾 Session Configuration
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
// ===============================
// 🔐 Authentication & Authorization
// ===============================
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
// ===============================
// 🚀 Build App
// ===============================

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
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
