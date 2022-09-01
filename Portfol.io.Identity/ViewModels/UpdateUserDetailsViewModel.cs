using AutoMapper;
using Portfol.io.Identity.Common.Mappings;
using Portfol.io.Identity.Models;
using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.ViewModels
{
    public class UpdateUserDetailsViewModel : IMapWith<AppUser>
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public DateTime DateOfBirth { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateUserDetailsViewModel, AppUser>()
                .ForMember(user => user.Id, opt => opt.MapFrom(vm => vm.Id))
                .ForMember(user => user.Name, opt => opt.MapFrom(vm => vm.Name))
                .ForMember(user => user.Description, opt => opt.MapFrom(vm => vm.Description))
                .ForMember(user => user.DateOfBirth, opt => opt.MapFrom(vm => vm.DateOfBirth));
        }
    }
}
