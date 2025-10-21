using Assignment02.Hubs;
using Business_Logic_Layer.Services;
using EVDealerDbContext.Models;
using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Services
{
    public class NotificationWrapperService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public NotificationWrapperService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Wrapper methods for OrderService
        public async Task NotifyOrderCreated(Order order)
        {
            await _hubContext.Clients.Group("staff").SendAsync("OrderCreated", 
                order.Id.ToString(), 
                order.Customer?.FullName ?? "Unknown", 
                order.Vehicle?.Name ?? "Unknown");
        }

        public async Task NotifyOrderCancelled(Order order, string reason)
        {
            await _hubContext.Clients.Group("staff").SendAsync("OrderCancelled", 
                order.Id.ToString(), 
                order.Customer?.FullName ?? "Unknown", 
                reason);
        }

        public async Task NotifyOrderStatusUpdate(Order order, string newStatus)
        {
            await _hubContext.Clients.Group($"customer_{order.CustomerId}").SendAsync("OrderStatusUpdated", 
                order.Id.ToString(), 
                newStatus);
            await _hubContext.Clients.Group("staff").SendAsync("OrderStatusUpdated", 
                order.Id.ToString(), 
                newStatus);
        }

        // Wrapper methods for VehicleService
        public async Task NotifyVehicleAdded(Vehicle vehicle)
        {
            await _hubContext.Clients.All.SendAsync("VehicleAdded", 
                vehicle.Id.ToString(), 
                vehicle.Name, 
                vehicle.Brand);
        }

        public async Task NotifyVehicleUpdated(Vehicle vehicle)
        {
            await _hubContext.Clients.All.SendAsync("VehicleUpdated", 
                vehicle.Id.ToString(), 
                vehicle.Name, 
                vehicle.Brand);
        }

        public async Task NotifyVehicleDeleted(Vehicle vehicle)
        {
            await _hubContext.Clients.All.SendAsync("VehicleDeleted", 
                vehicle.Id.ToString(), 
                vehicle.Name);
        }

        public async Task NotifyVehicleStockUpdate(Vehicle vehicle)
        {
            await _hubContext.Clients.All.SendAsync("VehicleStockUpdated", 
                vehicle.Id.ToString(), 
                vehicle.Name, 
                vehicle.StockQuantity ?? 0);
        }

        // Wrapper methods for UserService
        public async Task NotifyUserRegistered(User user)
        {
            await _hubContext.Clients.Group("staff").SendAsync("UserRegistered", 
                user.Id.ToString(), 
                user.Username, 
                user.Role);
        }

        public async Task NotifyUserUpdated(User user)
        {
            await _hubContext.Clients.All.SendAsync("UserUpdated", 
                user.Id.ToString(), 
                user.Username);
        }

        // Wrapper methods for TestDriveService
        public async Task NotifyTestDriveBooked(TestDriveAppointment appointment)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveBooked", 
                appointment.Id.ToString(), 
                appointment.Customer?.FullName ?? "Unknown", 
                appointment.Vehicle?.Name ?? "Unknown", 
                appointment.AppointmentDate);
        }

        public async Task NotifyTestDriveCancelled(TestDriveAppointment appointment)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveCancelled", 
                appointment.Id.ToString(), 
                appointment.Customer?.FullName ?? "Unknown", 
                appointment.Vehicle?.Name ?? "Unknown");
        }

        public async Task NotifyTestDriveCompleted(TestDriveAppointment appointment)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveCompleted", 
                appointment.Id.ToString(), 
                appointment.Customer?.FullName ?? "Unknown", 
                appointment.Vehicle?.Name ?? "Unknown");
        }
    }
}
