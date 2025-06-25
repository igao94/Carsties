using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService;

public class AuctionDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
}
