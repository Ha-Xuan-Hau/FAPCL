using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class RoomEquipment
    {
        public int EquipmentId { get; set; }
        public int RoomId { get; set; }
        public string EquipmentName { get; set; } = null!;
        public string? Status { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
