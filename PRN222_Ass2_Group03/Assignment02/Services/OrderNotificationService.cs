using Assignment02.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Services
{
    public class OrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotificationService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyOrderStatusUpdateAsync(string orderId, string newStatus, string customerId)
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("OrderStatusUpdated", orderId, newStatus);
            await _hubContext.Clients.Group("staff").SendAsync("OrderStatusUpdated", orderId, newStatus);
        }
    }
}
