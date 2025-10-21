using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task NotifyOrderUpdate(string orderId, string newStatus, string customerId)
        {
            Console.WriteLine($"OrderHub: Notifying order {orderId} status {newStatus} for customer {customerId}");
            
            // Gửi thông báo đến tất cả client trong group của customer
            await Clients.Group($"customer_{customerId}").SendAsync("OrderStatusUpdated", orderId, newStatus);
            Console.WriteLine($"OrderHub: Sent to customer group: customer_{customerId}");
            
            // Gửi thông báo đến tất cả admin/staff
            await Clients.Group("staff").SendAsync("OrderStatusUpdated", orderId, newStatus);
            Console.WriteLine($"OrderHub: Sent to staff group");
        }

        public async Task TestMessage(string message)
        {
            await Clients.All.SendAsync("TestMessage", $"Server received: {message}");
        }
    }
}
