using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FAPCL.Model
{
    public partial class BookClassRoomContext : DbContext
    {
        public BookClassRoomContext()
        {
        }

        public BookClassRoomContext(DbContextOptions<BookClassRoomContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; } = null!;
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; } = null!;
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; } = null!;
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; } = null!;
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; } = null!;
        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<BookingHistory> BookingHistories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<ClassSchedule> ClassSchedules { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Exam> Exams { get; set; } = null!;
        public virtual DbSet<ExamSchedule> ExamSchedules { get; set; } = null!;
        public virtual DbSet<News> News { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<RoomEquipment> RoomEquipments { get; set; } = null!;
        public virtual DbSet<RoomStatusLog> RoomStatusLogs { get; set; } = null!;
        public virtual DbSet<RoomType> RoomTypes { get; set; } = null!;
        public virtual DbSet<Slot> Slots { get; set; } = null!;
        public virtual DbSet<StudentClass> StudentClasses { get; set; } = null!;
        public virtual DbSet<Timetable> Timetables { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                                          .SetBasePath(Directory.GetCurrentDirectory())
                                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("ConnectionStrings"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetRoleClaim>(entity =>
            {
                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__AspNetRol__RoleI__3B75D760");
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Address).HasMaxLength(256);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.FirstName).HasMaxLength(128);

                entity.Property(e => e.LastName).HasMaxLength(128);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(128);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(128);

                entity.Property(e => e.UserName).HasMaxLength(128);

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId").HasConstraintName("FK__AspNetUse__RoleI__44FF419A"),
                        r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId").HasConstraintName("FK__AspNetUse__UserI__440B1D61"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId").HasName("PK__AspNetUs__AF2760AD971EAC7F");

                            j.ToTable("AspNetUserRoles");

                            j.IndexerProperty<string>("UserId").HasMaxLength(128);

                            j.IndexerProperty<string>("RoleId").HasMaxLength(128);
                        });
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__AspNetUse__UserI__3E52440B");
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey })
                    .HasName("PK__AspNetUs__2B2C5B52F2B1CA90");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__AspNetUse__UserI__412EB0B6");
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
                    .HasName("PK__AspNetUs__8CC49841D8B5DEC4");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__AspNetUse__UserI__47DBAE45");
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.BookingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Purpose).HasMaxLength(255);

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.SlotBookingDate).HasColumnType("date");

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Confirmed')");

                entity.Property(e => e.UserId)
                    .HasMaxLength(128)
                    .HasColumnName("UserID");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Bookings__RoomID__60A75C0F");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Bookings__SlotID__628FA481");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Bookings__UserID__619B8048");
            });

            modelBuilder.Entity<BookingHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryId)
                    .HasName("PK__Booking___4D7B4ADDC74A6B8E");

                entity.ToTable("Booking_History");

                entity.Property(e => e.HistoryId).HasColumnName("HistoryID");

                entity.Property(e => e.Action).HasMaxLength(50);

                entity.Property(e => e.ActionDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.ChangedBy).HasMaxLength(128);

                entity.HasOne(d => d.Booking)
                    .WithMany(p => p.BookingHistories)
                    .HasForeignKey(d => d.BookingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Booking_H__Booki__6754599E");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.ClassName).HasMaxLength(100);

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.TeacherId)
                    .HasMaxLength(128)
                    .HasColumnName("TeacherID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Classes__CourseI__70DDC3D8");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Classes__RoomID__72C60C4A");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Classes__SlotID__73BA3083");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Classes__Teacher__71D1E811");
            });

            modelBuilder.Entity<ClassSchedule>(entity =>
            {
                entity.Property(e => e.ClassScheduleId).HasColumnName("ClassScheduleID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.DayOfWeek).HasMaxLength(10);

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassSchedules)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassSche__Class__01142BA1");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.ClassSchedules)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ClassSche__SlotI__02084FDA");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.CourseName).HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(255);
            });

            modelBuilder.Entity<Exam>(entity =>
            {
                entity.Property(e => e.ExamId).HasColumnName("ExamID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.ExamDate).HasColumnType("date");

                entity.Property(e => e.ExamName).HasMaxLength(100);

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Exams)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Exams__CourseID__04E4BC85");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Exams)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Exams__RoomID__05D8E0BE");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.Exams)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Exams__SlotID__06CD04F7");
            });

            modelBuilder.Entity<ExamSchedule>(entity =>
            {
                entity.Property(e => e.ExamScheduleId).HasColumnName("ExamScheduleID");

                entity.Property(e => e.ExamDate).HasColumnType("date");

                entity.Property(e => e.ExamId).HasColumnName("ExamID");

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(128)
                    .HasColumnName("StudentID");

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.ExamSchedules)
                    .HasForeignKey(d => d.ExamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExamSched__ExamI__09A971A2");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.ExamSchedules)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExamSched__RoomI__0B91BA14");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.ExamSchedules)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExamSched__SlotI__0C85DE4D");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ExamSchedules)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExamSched__Stude__0A9D95DB");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.Property(e => e.NewsId).HasColumnName("NewsID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.IsPublished).HasDefaultValueSql("((1))");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__News__CreatedBy__114A936A");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.HasProjector).HasDefaultValueSql("((0))");

                entity.Property(e => e.HasSoundSystem).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsAction).HasDefaultValueSql("((1))");

                entity.Property(e => e.RoomName).HasMaxLength(100);

                entity.Property(e => e.RoomTypeId).HasColumnName("RoomTypeID");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Available')");

                entity.HasOne(d => d.RoomType)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.RoomTypeId)
                    .HasConstraintName("FK__Rooms__RoomTypeI__5629CD9C");
            });

            modelBuilder.Entity<RoomEquipment>(entity =>
            {
                entity.HasKey(e => e.EquipmentId)
                    .HasName("PK__Room_Equ__3447459935626F8C");

                entity.ToTable("Room_Equipment");

                entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");

                entity.Property(e => e.EquipmentName).HasMaxLength(100);

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Available')");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomEquipments)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Room_Equi__RoomI__5AEE82B9");
            });

            modelBuilder.Entity<RoomStatusLog>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PK__Room_Sta__5E5499A874C82393");

                entity.ToTable("Room_Status_Logs");

                entity.Property(e => e.LogId).HasColumnName("LogID");

                entity.Property(e => e.ChangedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ChangedBy).HasMaxLength(128);

                entity.Property(e => e.RoomId).HasColumnName("RoomID");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomStatusLogs)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Room_Stat__RoomI__6C190EBB");
            });

            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.HasIndex(e => e.RoomType1, "UQ__RoomType__3A76E8C3A5069969")
                    .IsUnique();

                entity.Property(e => e.RoomTypeId).HasColumnName("RoomTypeID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.RoomType1)
                    .HasMaxLength(50)
                    .HasColumnName("RoomType");
            });

            modelBuilder.Entity<Slot>(entity =>
            {
                entity.HasIndex(e => e.SlotName, "UQ__Slots__880B6BDFB3D2C968")
                    .IsUnique();

                entity.Property(e => e.SlotId)
                    .ValueGeneratedNever()
                    .HasColumnName("SlotID");

                entity.Property(e => e.SlotName).HasMaxLength(50);
            });

            modelBuilder.Entity<StudentClass>(entity =>
            {
                entity.Property(e => e.StudentClassId).HasColumnName("StudentClassID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.EnrollmentDate).HasColumnType("date");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Enrolled')");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(128)
                    .HasColumnName("StudentID");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.StudentClasses)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StudentCl__Class__797309D9");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.StudentClasses)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StudentCl__Stude__787EE5A0");
            });

            modelBuilder.Entity<Timetable>(entity =>
            {
                entity.ToTable("Timetable");

                entity.Property(e => e.TimetableId).HasColumnName("TimetableID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.DayOfWeek).HasMaxLength(10);

                entity.Property(e => e.SlotId).HasColumnName("SlotID");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(128)
                    .HasColumnName("StudentID");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Timetables)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Timetable__Class__7D439ABD");

                entity.HasOne(d => d.Slot)
                    .WithMany(p => p.Timetables)
                    .HasForeignKey(d => d.SlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Timetable__SlotI__7E37BEF6");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Timetables)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Timetable__Stude__7C4F7684");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
