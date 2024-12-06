using Microsoft.AspNetCore.SignalR;

namespace GreenGardenClient.Hubs
{
    public class CartHub : Hub 
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceviceMessage",$"{Context.ConnectionId} has connected");
        }
        public async Task UpdateCart(string connectionId)
        {
            await Clients.All.SendAsync("ReceiveCartUpdate", connectionId);
        }
    }
}
