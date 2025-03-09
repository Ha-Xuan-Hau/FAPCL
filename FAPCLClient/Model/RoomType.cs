using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class RoomType
    {
        public RoomType()
        {
            Rooms = new HashSet<Room>();
        }

        public int RoomTypeId { get; set; }
        public string RoomType1 { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }
    }
}
