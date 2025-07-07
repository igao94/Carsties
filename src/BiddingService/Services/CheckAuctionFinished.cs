using BiddingService.Entites;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished(ILogger<CheckAuctionFinished> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting check for finished auctions.");

        stoppingToken.Register(() => logger.LogInformation("==> Auction check is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(a => a.AuctionEnd < DateTime.UtcNow)
            .Match(a => !a.Finished)
            .ExecuteAsync(stoppingToken);

        if (!finishedAuctions.Any())
        {
            return;
        }

        logger.LogInformation("==> Found {count} auctions that have completed.", finishedAuctions.Count);

        using var scope = serviceProvider.CreateScope();

        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;

            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(b => b.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(b => b.Amount))
                .ExecuteFirstAsync(stoppingToken);

            var finishedAuction = new AuctionFinished
            {
                AuctionId = auction.ID,
                Seller = auction.Seller,
                Amount = winningBid?.Amount,
                ItemSold = winningBid is not null,
                Winner = winningBid?.Bidder
            };

            await endpoint.Publish(finishedAuction, stoppingToken);
        }
    }
}
