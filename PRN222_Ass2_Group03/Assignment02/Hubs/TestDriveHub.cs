using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Hubs
{
    public class TestDriveHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
