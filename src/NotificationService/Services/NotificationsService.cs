using Contracts;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Services;

public class NotificationsService(IHubContext<NotificationHub> hubContext) : INotificationsService
{
    public async Task NotifyAuctionCreatedAsync(AuctionCreated message)
    {
        await hubContext.Clients.All.SendAsync("AuctionCreated", message);
    }

    public async Task NotifyAuctionFinishedAsync(AuctionFinished message)
    {
        await hubContext.Clients.All.SendAsync("AuctionFinished", message);
    }

    public async Task NotifyBidPlacedAsync(BidPlaced message)
    {
        await hubContext.Clients.All.SendAsync("BidPlaced", message);
    }
}
