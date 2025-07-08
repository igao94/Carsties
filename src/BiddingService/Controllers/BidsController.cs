using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entites;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController(IMapper mapper,
    GrpcAuctionClient grpcClient,
    IPublishEndpoint publishEndpoint) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);

        if (auction is null)
        {
            auction = grpcClient.GetAuction(auctionId);

            if (auction is null)
            {
                return BadRequest("Cannot accept bids on this auction at this time.");
            }
        }

        if (auction.Seller == User.Identity?.Name)
        {
            return BadRequest("You cannot bid on your own auction.");
        }

        var bid = new Bid
        {
            AuctionId = auction.ID,
            Bidder = User.Identity?.Name!,
            Amount = amount
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highBid = await DB.Find<Bid>()
                .Match(b => b.AuctionId == auction.ID)
                .Sort(x => x.Descending(b => b.Amount))
                .ExecuteFirstAsync();

            if (highBid is not null && bid.Amount > highBid.Amount || highBid is null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted
                    : BidStatus.AcceptedBelowReserve;
            }

            if (highBid is not null && bid.Amount <= highBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }

        await DB.SaveAsync(bid);

        await publishEndpoint.Publish(mapper.Map<BidPlaced>(bid));

        return Ok(mapper.Map<BidDto>(bid));
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(x => x.Descending(b => b.BidTime))
            .ExecuteAsync();

        return bids.Select(mapper.Map<BidDto>).ToList();
    }
}
