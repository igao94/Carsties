﻿using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();

        _auctionRepo = new Mock<IAuctionRepository>();

        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);

        _controller = new AuctionsController(_auctionRepo.Object, _publishEndpoint.Object, _mapper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = Helpers.GetClaimsPrincipal()
                }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();

        _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // act
        var result = await _controller.GetAllAuctions(null);

        // assert
        Assert.Equal(10, result.Value?.Count);

        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctions_WithDate_Returns10Auctions()
    {
        // arrange
        var date = "2023-12-15T10:30:00Z";

        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();

        _auctionRepo.Setup(repo => repo.GetAuctionsAsync(date)).ReturnsAsync(auctions);

        // act
        var result = await _controller.GetAllAuctions(date);

        // assert
        Assert.Equal(10, result.Value?.Count);

        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.GetAuctionById(auction.Id);

        // assert
        Assert.Equal(auction.Make, result.Value?.Make);

        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    
    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();

        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));

        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.CreateAuction(auction);

        var createdAtResult = result.Result as CreatedAtActionResult;

        // assert
        Assert.NotNull(createdAtResult);

        Assert.Equal("GetAuctionById", createdAtResult.ActionName);

        Assert.IsType<AuctionDto>(createdAtResult.Value);
    }


    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();

        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));

        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // act
        var result = await _controller.CreateAuction(auction);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();

        auction.Seller = "test";

        var updateAuction = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuction);

        // assert 
        Assert.IsType<OkResult>(result);
    }


    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        auction.Seller = "non-test";

        var updateAuction = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuction);

        // assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        var updateAuction = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuction);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task UpdateAuction_FailedSave_ReturnsBadRequest()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        auction.Item = _fixture.Build<Item>().Without(i => i.Auction).Create();

        auction.Seller = "test";

        var updateAuction = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateAuction);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        auction.Seller = "test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert 
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        // arrange
        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(a => a.Item).Create();

        auction.Seller = "non-test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.DeleteAuction(auction.Id);

        // assert
        Assert.IsType<ForbidResult>(result);
    }
}
