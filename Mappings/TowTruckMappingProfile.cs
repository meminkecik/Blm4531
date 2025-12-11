using AutoMapper;
using Nearest.DTOs.TowTruck;
using Nearest.Models;

namespace Nearest.Mappings
{
	public class TowTruckMappingProfile : Profile
	{
		public TowTruckMappingProfile()
		{
			CreateMap<TowTruck, TowTruckDto>()
				.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : ""))
				.ForMember(dest => dest.CompanyPhone, opt => opt.MapFrom(src => src.Company != null ? src.Company.PhoneNumber : ""))
				.ForMember(dest => dest.Distance, opt => opt.Ignore()); // Manuel olarak hesaplanacak
			CreateMap<TowTruckArea, TowTruckAreaDto>();
		}
		}
}


