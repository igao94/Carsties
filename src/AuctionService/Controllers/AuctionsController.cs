using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController(IAuctionRepository auctionRepository,
    IPublishEndpoint publishEndpoint,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
    {
        return await auctionRepository.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto?>> GetAuctionById(Guid id)
    {
        var auction = await auctionRepository.GetAuctionByIdAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        return auction;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = mapper.Map<Auction>(createAuctionDto);

        auction.Seller = User.Identity?.Name ?? "Unknown user";

        auctionRepository.AddAuction(auction);

        var newAuction = mapper.Map<AuctionDto>(auction);

        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));

        var result = await auctionRepository.SaveChangesAsync();

        if (!result)
        {
            return BadRequest("Could not save changes to DB.");
        }

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await auctionRepository.GetAuctionEntityAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

        var result = await auctionRepository.SaveChangesAsync();

        return result
            ? Ok()
            : BadRequest("Problem saving changes.");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await auctionRepository.GetAuctionEntityAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        auctionRepository.RemoveAuction(auction);

        await publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await auctionRepository.SaveChangesAsync();

        return result
            ? Ok()
            : BadRequest("Problem deleting auction.");
    }
}
