using Contracts;
using MassTransit;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class BidPlacedConsumer(INotificationsService notificationService) : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("==> Bid placed message received.");

        await notificationService.NotifyBidPlacedAsync(context.Message);
    }
}
