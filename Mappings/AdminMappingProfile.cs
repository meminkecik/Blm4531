using AutoMapper;
using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Mappings
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<Admin, AdminDto>();
        }
    }
}
