using AutoMapper;
using Portfol.io.Identity.Common.Mappings;
using Portfol.io.Identity.Models;

namespace Portfol.io.Identity.DTO.ResponseModels.UserResponseModel
{
    public class UserShortDetailsResponse : IMapWith<AppUser>
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ProfileImagePath { get; set; } = null!;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppUser, UserShortDetailsResponse>()
                .ForMember(vm => vm.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(vm => vm.Username, opt => opt.MapFrom(user => user.UserName))
                .ForMember(vm => vm.FirstName, opt => opt.MapFrom(user => user.FirstName))
                .ForMember(vm => vm.LastName, opt => opt.MapFrom(user => user.LastName))
                .ForMember(vm => vm.ProfileImagePath, opt => opt.MapFrom(user => user.ProfileImagePath));
        }
    }
}
