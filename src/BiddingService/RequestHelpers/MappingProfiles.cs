using AutoMapper;
using BiddingService.Consumers;
using BiddingService.DTOs;
using BiddingService.Entites;
using Contracts;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();

        CreateMap<Bid, BidPlaced>();
    }
}
