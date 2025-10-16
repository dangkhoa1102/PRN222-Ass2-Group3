using Assignment02.Hubs;
using Business_Logic_Layer.Interfaces;
using Business_Logic_Layer.Services;
using DataAccess_Layer;
using DataAccess_Layer.Repositories;
using DataAccess_Layer.Repositories.Implement;
using DataAccess_Layer.Repositories.Interface;
using EVDealerDbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages & SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Add EF DbContext
builder.Services.AddDbContext<EVDealerSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));

// Register Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
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

// ✅ Map Routes & Hubs
app.MapRazorPages();
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
