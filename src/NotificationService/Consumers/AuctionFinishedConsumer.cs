using Contracts;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class AuctionFinishedConsumer(INotificationsService notificationService) : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("==> Auction finished message received.");

        await notificationService.NotifyAuctionFinishedAsync(context.Message);
    }
}
