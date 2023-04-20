using AutoMapper;
using Portfol.io.Identity.Common.Mappings;
using Portfol.io.Identity.Models;

namespace Portfol.io.Identity.DTO
{
    public class UserDto : IMapWith<AppUser>
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string MiddleName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Description { get; set; }
        public string ProfileImagePath { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfCreation { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AppUser, UserDto>()
                .ForMember(lookUp => lookUp.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(lookUp => lookUp.UserName, opt => opt.MapFrom(user => user.UserName))
                .ForMember(lookUp => lookUp.FullName, opt => opt.MapFrom(user => user.FullName))
                .ForMember(lookUp => lookUp.FirstName, opt => opt.MapFrom(user => user.FirstName))
                .ForMember(lookUp => lookUp.LastName, opt => opt.MapFrom(user => user.LastName))
                .ForMember(lookUp => lookUp.MiddleName, opt => opt.MapFrom(user => user.MiddleName))
                .ForMember(lookUp => lookUp.Email, opt => opt.MapFrom(user => user.Email))
                .ForMember(lookUp => lookUp.Phone, opt => opt.MapFrom(user => user.PhoneNumber))
                .ForMember(lookUp => lookUp.Description, opt => opt.MapFrom(user => user.Description))
                .ForMember(lookUp => lookUp.ProfileImagePath, opt => opt.MapFrom(user => user.ProfileImagePath))
                .ForMember(lookUp => lookUp.DateOfBirth, opt => opt.MapFrom(user => user.DateOfBirth))
                .ForMember(lookUp => lookUp.DateOfCreation, opt => opt.MapFrom(user => user.DateOfCreation));
        }
    }
}
