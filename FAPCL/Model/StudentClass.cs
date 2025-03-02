using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class StudentClass
    {
        public int StudentClassId { get; set; }
        public string StudentId { get; set; } = null!;
        public int ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Status { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual AspNetUser Student { get; set; } = null!;
    }
}
