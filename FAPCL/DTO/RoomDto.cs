using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
        public int RoomTypeId { get; set; }
        public bool? HasProjector { get; set; }
        public bool? HasSoundSystem { get; set; }
        public string? Status { get; set; }
    }
}
