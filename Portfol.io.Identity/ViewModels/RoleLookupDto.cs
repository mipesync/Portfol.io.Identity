using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Portfol.io.Identity.Common.Mappings;

namespace Portfol.io.Identity.ViewModels
{
    public class RoleLookupDto : IMapWith<IdentityRole>
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<IdentityRole, RoleLookupDto>()
                .ForMember(roleLookup => roleLookup.Id, opt => opt.MapFrom(role => role.Id))
                .ForMember(roleLookup => roleLookup.Name, opt => opt.MapFrom(role => role.Name));
        }
    }
}
