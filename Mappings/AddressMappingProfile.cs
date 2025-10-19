using AutoMapper;
using Nearest.DTOs.Address;
using Nearest.Models.Address;

namespace Nearest.Mappings
{
    public class AddressMappingProfile : Profile
    {
        public AddressMappingProfile()
        {
            CreateMap<City, CityDto>();
            CreateMap<District, DistrictDto>();
            CreateMap<CityDistrict, DistrictDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.District.Id))
                .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.District.DistrictId))
                .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.DistrictName));
        }
    }
}
