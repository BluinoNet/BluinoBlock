using Microsoft.AspNetCore.SignalR;

namespace BluinoBlock.Data
{
    public class DeviceHub : Hub
    {
        public async Task SendMessage(string method, string param1, string param2)
        {
            await Clients.All.SendAsync("ReceiveMessage", method,param1,param2);
        }
        public bool CheckConnection()
        {
            return true;
        }

        public void ReportStatus(string status)
        {
            Console.WriteLine($"{Context.ConnectionId}:{status}");
            //do nothing
        }
    }
}