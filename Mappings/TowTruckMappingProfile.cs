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
				.ForMember(dest => dest.CompanyEmail, opt => opt.MapFrom(src => src.Company != null ? src.Company.Email : null))
				.ForMember(dest => dest.CompanyAddress, opt => opt.MapFrom(src => src.Company != null ? src.Company.FullAddress : null))
				.ForMember(dest => dest.CompanyCity, opt => opt.MapFrom(src => src.Company != null ? src.Company.City : null))
				.ForMember(dest => dest.CompanyDistrict, opt => opt.MapFrom(src => src.Company != null ? src.Company.District : null))
				.ForMember(dest => dest.Distance, opt => opt.Ignore()) // Manuel olarak hesaplanacak
				.ForMember(dest => dest.IsCompanyOnly, opt => opt.MapFrom(src => false)); // Çekici kaydı
			CreateMap<TowTruckArea, TowTruckAreaDto>();
		}
		}
}


