using AutoMapper;
using FAPCL.DTO;
using FAPCL.Model;

namespace FAPCL.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Booking, BookingDTO>()
                .ForMember(x => x.RoomName, y => y.MapFrom(z => z.Room.RoomName))
                .ForMember(x => x.SlotNumber, y => y.MapFrom(z => z.Slot.SlotName))
                .ForMember(x => x.UserEmail, y => y.MapFrom(z => z.User.Email))
                .ReverseMap();
        }
    }
}
