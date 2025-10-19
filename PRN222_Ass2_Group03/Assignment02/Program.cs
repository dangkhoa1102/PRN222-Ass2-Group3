using Assignment02.Hubs;
using Business_Logic_Layer.Services;
using DataAccess_Layer;
using DataAccess_Layer.Repositories;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 🔧 Add Razor Pages & SignalR
// ===============================
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// ===============================
// 🗄️ Configure Entity Framework DbContext
// ===============================
builder.Services.AddDbContext<EVDealerSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));

// ===============================
// 💡 Register Repositories & Services
// ===============================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ICustomerTestDriveAppointment, CustomerTestDriveAppointment>();
builder.Services.AddScoped<ICustomerTestDriveAppointmentService, CustomerTestDriveAppointmentService>();

// ===============================
// 💾 Session Configuration
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
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
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ===============================
// 🚀 Build App
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
