using Contracts;

namespace NotificationService.Services;

public interface INotificationsService
{
    Task NotifyAuctionCreatedAsync(AuctionCreated message);
    Task NotifyAuctionFinishedAsync(AuctionFinished message);
    Task NotifyBidPlacedAsync(BidPlaced message);
}
