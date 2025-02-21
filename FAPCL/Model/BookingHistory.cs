using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class BookingHistory
    {
        public int HistoryId { get; set; }
        public int BookingId { get; set; }
        public string Action { get; set; } = null!;
        public DateTime? ActionDate { get; set; }
        public string? ChangedBy { get; set; }

        public virtual Booking Booking { get; set; } = null!;
    }
}
