using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class RoomStatusLog
    {
        public int LogId { get; set; }
        public int RoomId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ChangedAt { get; set; }
        public string? ChangedBy { get; set; }

        public virtual Room Room { get; set; } = null!;
    }
}
