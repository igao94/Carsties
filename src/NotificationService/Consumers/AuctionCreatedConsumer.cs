using Contracts;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer(INotificationsService notificationService) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("==> Auction created message received.");

        await notificationService.NotifyAuctionCreatedAsync(context.Message);
    }
}
