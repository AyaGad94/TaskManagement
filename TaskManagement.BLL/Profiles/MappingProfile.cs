using AutoMapper;
using TaskManagement.BLL.DTOs;
using TaskManagement.DAL.Entities;

namespace TaskManagement.BLL.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskItem, TaskDto>();

            CreateMap<TaskCreateDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        }
    }
}

