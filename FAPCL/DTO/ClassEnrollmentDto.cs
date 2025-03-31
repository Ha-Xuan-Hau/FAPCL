using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class ClassEnrollmentDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RoomId { get; set; }
        public string RoomName { get; set; }

        public int Capacity { get; set; }
        public int RegisteredCount { get; set; }

        public int AvailableSlots => Capacity - RegisteredCount;
    }
}
