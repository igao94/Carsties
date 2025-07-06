using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entites;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();
    }
}
