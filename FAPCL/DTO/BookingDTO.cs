namespace FAPCL.DTO
{
    public class BookingRequest
    {
        public int RoomId { get; set; }
        public int SlotId { get; set; }
        public DateTime SelectedDate { get; set; }
        public string UserId { get; set; }
        public string Purpose { get; set; }
    }

    public class CancelBookingRequest
    {
        public int BookingId { get; set; }
    }
}
