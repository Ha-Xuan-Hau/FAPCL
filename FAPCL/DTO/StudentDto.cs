using Microsoft.AspNetCore.Mvc;

namespace FAPCL.DTO
{
    public class StudentDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
