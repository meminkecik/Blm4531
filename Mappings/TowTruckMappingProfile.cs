using AutoMapper;
using Nearest.DTOs.TowTruck;
using Nearest.Models;

namespace Nearest.Mappings
{
	public class TowTruckMappingProfile : Profile
	{
		public TowTruckMappingProfile()
		{
			CreateMap<TowTruck, TowTruckDto>();
			CreateMap<TowTruckArea, TowTruckAreaDto>();
		}
	}
}


