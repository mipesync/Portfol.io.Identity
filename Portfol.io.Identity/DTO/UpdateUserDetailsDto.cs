using AutoMapper;
using Portfol.io.Identity.Common.Mappings;
using Portfol.io.Identity.Models;
using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.DTO
{
    public class UpdateUserDetailsDto : IMapWith<AppUser>
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateUserDetailsDto, AppUser>()
                .ForMember(user => user.FullName, opt => opt.MapFrom(vm => vm.Name))
                .ForMember(user => user.Description, opt => opt.MapFrom(vm => vm.Description))
                .ForMember(user => user.DateOfBirth, opt => opt.MapFrom(vm => vm.DateOfBirth));
        }
    }
}
