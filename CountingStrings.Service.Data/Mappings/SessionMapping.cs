using AutoMapper;
using CountingStrings.API.Contract;
using CountingStrings.Service.Data.Models;

namespace CountingStrings.Service.Data.Mappings
{
    public class SessionMapping : Profile
    {
        public SessionMapping()
        {
            CreateMap<OpenSession, Session>()
                .ForMember(target => target.Id, opt => opt.MapFrom(s => s.SessionId));
        }
    }
}
