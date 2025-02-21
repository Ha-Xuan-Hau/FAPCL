using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class Booking
    {
        public Booking()
        {
            BookingHistories = new HashSet<BookingHistory>();
        }

        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public string UserId { get; set; } = null!;
        public string? Purpose { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime SlotBookingDate { get; set; }
        public string? Status { get; set; }

        public virtual Room Room { get; set; } = null!;
        public virtual Slot Slot { get; set; } = null!;
        public virtual AspNetUser User { get; set; } = null!;
        public virtual ICollection<BookingHistory> BookingHistories { get; set; }
    }
}
