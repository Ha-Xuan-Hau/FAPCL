using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class AspNetUser : IdentityUser
    {
        public AspNetUser()
        {
            Bookings = new HashSet<Booking>();
            Classes = new HashSet<Class>();
            ExamSchedulesAsStudent = new HashSet<ExamSchedule>();
            ExamSchedulesAsTeacher = new HashSet<ExamSchedule>();
            News = new HashSet<News>();
            StudentClasses = new HashSet<StudentClass>();
            Timetables = new HashSet<Timetable>();
        }

        //public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        //public string UserName { get; set; } = null!;
        //public string? NormalizedUserName { get; set; }
        //public string? Email { get; set; }
        //public string? NormalizedEmail { get; set; }
        //public bool EmailConfirmed { get; set; }
        //public string? PasswordHash { get; set; }
        //public string? SecurityStamp { get; set; }
        //public string? ConcurrencyStamp { get; set; }
        //public string? PhoneNumber { get; set; }
        //public bool PhoneNumberConfirmed { get; set; }
        //public bool TwoFactorEnabled { get; set; }
        //public DateTimeOffset? LockoutEnd { get; set; }
        //public bool LockoutEnabled { get; set; }
        //public int AccessFailedCount { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
        // public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<StudentClass> StudentClasses { get; set; }
        public virtual ICollection<Timetable> Timetables { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedulesAsStudent { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedulesAsTeacher { get; set; }
    }
}
