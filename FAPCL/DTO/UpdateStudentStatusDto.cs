namespace FAPCL.DTO
{
    public class UpdateStudentStatusDto
    {
        public string StudentId { get; set; }
        public int ClassId { get; set; }
        public string Status { get; set; }
    }
}
