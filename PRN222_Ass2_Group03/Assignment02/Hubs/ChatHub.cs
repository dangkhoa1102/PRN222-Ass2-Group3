using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoined", $"{Context.User?.Identity?.Name ?? Context.ConnectionId} joined the group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", $"{Context.User?.Identity?.Name ?? Context.ConnectionId} left the group {groupName}");
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", $"User {Context.ConnectionId} connected");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserDisconnected", $"User {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}