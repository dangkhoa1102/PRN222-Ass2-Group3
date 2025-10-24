using Microsoft.AspNetCore.SignalR;
using Assignment02.Hubs;

namespace Assignment02.Services
{
    public class RealTimeNotificationService
    {
        private readonly IHubContext<RealTimeHub> _hubContext;

        public RealTimeNotificationService(IHubContext<RealTimeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Gửi thông báo reload trang cho tất cả clients
        public async Task NotifyPageReload(string pageName, string action = "update")
        {
            Console.WriteLine($"Sending PageReload notification: {pageName} - {action}");
            await _hubContext.Clients.All.SendAsync("PageReload", new
            {
                page = pageName,
                action = action,
                timestamp = DateTime.Now
            });
            Console.WriteLine("PageReload notification sent successfully");
        }

        // Gửi thông báo reload cho một group cụ thể (staff hoặc customer)
        public async Task NotifyGroupReload(string groupName, string pageName, string action = "update")
        {
            await _hubContext.Clients.Group(groupName).SendAsync("PageReload", new
            {
                page = pageName,
                action = action,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi có order mới
        public async Task NotifyOrderCreated(string orderNumber, string customerName, string vehicleName)
        {
            await _hubContext.Clients.All.SendAsync("OrderCreated", new
            {
                orderNumber = orderNumber,
                customerName = customerName,
                vehicleName = vehicleName,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi order được cập nhật
        public async Task NotifyOrderUpdated(string orderNumber, string status)
        {
            await _hubContext.Clients.All.SendAsync("OrderUpdated", new
            {
                orderNumber = orderNumber,
                status = status,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi payment status được cập nhật
        public async Task NotifyPaymentUpdated(string orderNumber, string paymentStatus, string customerName)
        {
            await _hubContext.Clients.All.SendAsync("PaymentUpdated", new
            {
                orderNumber = orderNumber,
                paymentStatus = paymentStatus,
                customerName = customerName,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi có test drive appointment mới
        public async Task NotifyTestDriveBooked(string customerName, string vehicleName, DateTime appointmentDate)
        {
            await _hubContext.Clients.All.SendAsync("TestDriveBooked", new
            {
                customerName = customerName,
                vehicleName = vehicleName,
                appointmentDate = appointmentDate,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi test drive được cập nhật
        public async Task NotifyTestDriveUpdated(string customerName, string vehicleName, string status)
        {
            await _hubContext.Clients.All.SendAsync("TestDriveUpdated", new
            {
                customerName = customerName,
                vehicleName = vehicleName,
                status = status,
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi test drive bị hủy
        public async Task NotifyTestDriveCancelled(string customerName, string vehicleName)
        {
            Console.WriteLine($"Sending TestDriveCancelled notification: {customerName} - {vehicleName}");
            await _hubContext.Clients.All.SendAsync("TestDriveCancelled", new
            {
                customerName = customerName,
                vehicleName = vehicleName,
                timestamp = DateTime.Now
            });
            Console.WriteLine("TestDriveCancelled notification sent successfully");
        }

        // Gửi thông báo khi vehicle được cập nhật
        public async Task NotifyVehicleUpdated(string vehicleName, string action)
        {
            await _hubContext.Clients.All.SendAsync("VehicleUpdated", new
            {
                vehicleName = vehicleName,
                action = action, // "added", "updated", "deleted"
                timestamp = DateTime.Now
            });
        }

        // Gửi thông báo khi khách hàng đánh dấu đơn hàng là DONE
        public async Task NotifyOrderMarkedDone(string orderNumber, string customerName, string vehicleName)
        {
            Console.WriteLine($"Sending OrderMarkedDone notification: {orderNumber} - {customerName} - {vehicleName}");
            await _hubContext.Clients.All.SendAsync("OrderMarkedDone", new
            {
                orderNumber = orderNumber,
                customerName = customerName,
                vehicleName = vehicleName,
                timestamp = DateTime.Now
            });
            Console.WriteLine("OrderMarkedDone notification sent successfully");
        }

        // Gửi thông báo khi order bị hủy
        public async Task NotifyOrderCancelled(string orderNumber, string customerName, string vehicleName)
        {
            Console.WriteLine($"Sending OrderCancelled notification: {orderNumber} - {customerName} - {vehicleName}");
            await _hubContext.Clients.All.SendAsync("OrderCancelled", new
            {
                orderNumber = orderNumber,
                customerName = customerName,
                vehicleName = vehicleName,
                timestamp = DateTime.Now
            });
            Console.WriteLine("OrderCancelled notification sent successfully");
        }
    }
}
