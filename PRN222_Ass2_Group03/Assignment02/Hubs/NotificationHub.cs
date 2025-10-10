using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string title, string message, string type = "info")
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message, type);
        }

        public async Task SendNotificationToUser(string userId, string title, string message, string type = "info")
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, message, type);
        }

        public async Task BroadcastSystemMessage(string message)
        {
            await Clients.All.SendAsync("SystemMessage", message, DateTime.Now);
        }
    }
}