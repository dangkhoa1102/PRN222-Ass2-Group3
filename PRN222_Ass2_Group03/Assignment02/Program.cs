using Assignment02.Hubs;
using Business_Logic_Layer.Services;
using DataAccess_Layer.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Register repositories and services (they will use EVDealerSystemContext's own connection)
// Thêm d?ch v? DbContext
builder.Services.AddDbContext<EVDealerDbContext.EVDealerSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITestDriveRepository, TestDriveRepository>();
builder.Services.AddScoped<ITestDriveService, TestDriveService>();
builder.Services.AddSignalR();
// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Custom middleware to redirect unauthenticated users to login
app.Use(async (context, next) =>
{
    // Check if user is accessing root path
    if (context.Request.Path == "/")
    {
        // Check if user is authenticated
        var userId = context.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            // Redirect to login page
            context.Response.Redirect("/Login");
            return;
        }
        else
        {
            // Redirect to index page if authenticated
            context.Response.Redirect("/Index");
            return;
        }
    }
    await next();
});

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<TestDriveHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
