using AutoMapper;
using SyntheticArbitrage.DAL.Entities;
using SyntheticArbitrage.Infrastructure.Utils;
using SyntheticArbitrage.Shared.ApiModels.Price;
using SyntheticArbitrage.Shared.Model;

namespace SyntheticArbitrage.API.Mapping;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<BinanceQBQDiffPrice, BinanceQBQDiffPriceResponseAM>();
        CreateMap<BinanceTickerPriceResponse, BinanceTickerPriceAM>()
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTimeUtil.GetDTFromUnix(src.Time)));
    }
}
