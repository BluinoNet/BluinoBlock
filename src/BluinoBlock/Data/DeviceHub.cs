using Microsoft.AspNetCore.SignalR;

namespace BluinoBlock.Data
{
    public class DeviceHub : Hub
    {
        public async Task SendMessage(string deviceid, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", deviceid, message);
        }
    }
}