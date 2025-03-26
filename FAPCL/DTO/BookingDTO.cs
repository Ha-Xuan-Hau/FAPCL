using FAPCL.Model;

namespace FAPCL.DTO
{
    public class BookingRequest
    {
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime SelectedDate { get; set; }
        public string Purpose { get; set; }
    }

    public class CancelBookingRequest
    {
        public int BookingId { get; set; }
    }

    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? Purpose { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime SlotBookingDate { get; set; }
        public string? Status { get; set; }
        public string RoomName { get; set; }
        public string SlotNumber { get; set; }
    }
}
