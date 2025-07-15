using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository(AuctionDbContext context, IMapper mapper) : IAuctionRepository
{
    public void AddAuction(Auction auction) => context.Auctions.Add(auction);

    public async Task<AuctionDto?> GetAuctionByIdAsync(Guid id)
    {
        return await context.Auctions
            .ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Auction?> GetAuctionEntityAsync(Guid id)
    {
        return await context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AuctionDto>> GetAuctionsAsync(string? date)
    {
        var query = context.Auctions
            .OrderBy(a => a.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query
            .ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void RemoveAuction(Auction auction) => context.Auctions.Remove(auction);

    public async Task<bool> SaveChangesAsync() => await context.SaveChangesAsync() > 0;
}
