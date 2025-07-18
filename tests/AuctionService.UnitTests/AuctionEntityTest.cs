using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTest
{
    [Fact]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        // arrange
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 10
        };

        // act
        var result = auction.HasReservePrice();

        // assertion
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceIsZero_False()
    {
        // arrange
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 0
        };

        // act
        var result = auction.HasReservePrice();

        // assertion
        Assert.False(result);
    }
}