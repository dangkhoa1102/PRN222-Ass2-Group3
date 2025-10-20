using Assignment02.Hubs;
using Business_Logic_Layer.Services;

// Add Razor Pages & SignalR
var builder = WebApplication.CreateBuilder(args);

// ===============================
// Add EF DbContext
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Register Service Factory
builder.Services.AddScoped<ServiceFactory>();

// Register Services through Factory
builder.Services.AddScoped<IUserService>(provider => provider.GetRequiredService<ServiceFactory>().CreateUserService());
builder.Services.AddScoped<IOrderService>(provider => provider.GetRequiredService<ServiceFactory>().CreateOrderService());
builder.Services.AddScoped<ICustomerTestDriveAppointmentService>(provider => provider.GetRequiredService<ServiceFactory>().CreateCustomerTestDriveAppointmentService());
builder.Services.AddScoped<IVehicleService>(provider => provider.GetRequiredService<ServiceFactory>().CreateVehicleService());

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
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
