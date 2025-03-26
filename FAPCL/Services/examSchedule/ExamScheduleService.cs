using FAPCL.DTO;
using FAPCL.DTO.ExamSchedule;
using FAPCL.Model;
using Microsoft.EntityFrameworkCore;

namespace FAPCL.Services.examSchedule
{
    public class ExamScheduleService : IExamScheduleService
    {
        private readonly BookClassRoomContext _context;
        private readonly ILogger<ExamScheduleService> _logger;

        public ExamScheduleService(
            BookClassRoomContext context,
            ILogger<ExamScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SchedulingResult> ScheduleExamsAsync(
    string examName,
    List<int> courseIds,
    DateTime startDate,
    DateTime endDate)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validate input and check if courses exist
                var courses = await _context.Courses
                    .Where(c => courseIds.Contains(c.CourseId))
                    .ToListAsync();

                if (courses.Count != courseIds.Count)
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "One or more selected courses do not exist"
                    };
                }

                // 2. Get all students enrolled in these courses
                var courseStudentMap = await GetEnrolledStudentsForCoursesAsync(courseIds);

                if (courseStudentMap.Values.Any(list => !list.Any()))
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "One or more courses have no enrolled students"
                    };
                }

                // 3. Get available time slots in the selected date range
                var availableSlots = await GetAvailableSlotsInDateRangeAsync(startDate, endDate);

                if (!availableSlots.Any())
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "No available time slots in the selected date range"
                    };
                }

                // 4. Get available rooms with RoomTypeId = 1
                var availableRooms = await GetAvailableRoomsAsync();

                if (!availableRooms.Any())
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "No available rooms for scheduling"
                    };
                }

                // 5. Build conflict graph between courses based on common students
                var conflictGraph = BuildCourseConflictGraph(courseStudentMap);

                // 6. Generate scheduling plan using graph coloring algorithm
                var schedulingPlan = GenerateSchedulingPlan(
                    courses,
                    conflictGraph,
                    courseStudentMap,
                    availableSlots,
                    availableRooms);

                if (!schedulingPlan.IsValid)
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = schedulingPlan.ErrorMessage
                    };
                }

                // Generate a unique session identifier to group these exams
                string sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string sessionExamName = $"{examName} [Session:{sessionId}]";

                // Store created exam IDs
                List<int> createdExamIds = new List<int>();

                // 7. Create exams and exam schedules
                // foreach (var assignment in schedulingPlan.CourseAssignments)
                // {
                //     var courseId = assignment.Key;
                //     var scheduleAssignment = assignment.Value;

                //     // Create the exam record
                //     var exam = new Exam
                //     {
                //         ExamName = sessionExamName,
                //         CourseId = courseId,
                //         RoomId = scheduleAssignment.RoomId,
                //         SlotId = scheduleAssignment.SlotInfo.SlotId,
                //         ExamDate = scheduleAssignment.SlotInfo.Date,
                //     };

                //     _context.Exams.Add(exam);
                //     await _context.SaveChangesAsync(); // Save to get ExamID
                //     createdExamIds.Add(exam.ExamId);

                //     // Assign a teacher/proctor for this exam
                //     // Assign a teacher/proctor for this exam
                //     var proctor = await GetAvailableProctorAsync(scheduleAssignment.SlotInfo.Date, scheduleAssignment.SlotInfo.SlotId);

                //     if (proctor == null)
                //     {
                //         await transaction.RollbackAsync();
                //         return new SchedulingResult
                //         {
                //             Success = false,
                //             Message = $"No available proctor for {courses.FirstOrDefault(c => c.CourseId == courseId)?.CourseName} exam"
                //         };
                //     }

                //     // Proceed to create exam schedules using the retrieved proctor
                //     foreach (var studentId in scheduleAssignment.StudentIds)
                //     {
                //         var examSchedule = new ExamSchedule
                //         {
                //             ExamId = exam.ExamId,
                //             StudentId = studentId,
                //             TeacherId = proctor.Id,
                //             RoomId = scheduleAssignment.RoomId,
                //             SlotId = scheduleAssignment.SlotInfo.SlotId,
                //             ExamDate = scheduleAssignment.SlotInfo.Date
                //         };

                //         _context.ExamSchedules.Add(examSchedule);
                //     }


                //     await _context.SaveChangesAsync(); // Save exam schedules
                // }

                // await transaction.CommitAsync();
                // 7. Create exams and exam schedules, splitting students into groups of 15 per exam room
                foreach (var assignment in schedulingPlan.CourseAssignments)
                {
                    var courseId = assignment.Key;
                    var scheduleAssignment = assignment.Value;
                    var studentIds = scheduleAssignment.StudentIds;

                    // Split the students into chunks of 15
                    var studentChunks = Chunk(studentIds, 15);

                    foreach (var chunk in studentChunks)
                    {
                        // Pick a random available room (classroom only)
                        var random = new Random();
                        var randomRoom = availableRooms[random.Next(availableRooms.Count)];

                        // Create the exam record for this chunk
                        var exam = new Exam
                        {
                            ExamName = sessionExamName,
                            CourseId = courseId,
                            RoomId = randomRoom.RoomId,
                            SlotId = scheduleAssignment.SlotInfo.SlotId,
                            ExamDate = scheduleAssignment.SlotInfo.Date,
                        };

                        _context.Exams.Add(exam);
                        await _context.SaveChangesAsync(); // Save to generate ExamID
                        createdExamIds.Add(exam.ExamId);

                        // Get a proctor for this exam slot
                        var proctor = await GetAvailableProctorAsync(scheduleAssignment.SlotInfo.Date, scheduleAssignment.SlotInfo.SlotId);
                        if (proctor == null)
                        {
                            await transaction.RollbackAsync();
                            return new SchedulingResult
                            {
                                Success = false,
                                Message = $"No available proctor for course {courses.FirstOrDefault(c => c.CourseId == courseId)?.CourseName} exam"
                            };
                        }

                        // Create exam schedule entries for each student in the current chunk
                        foreach (var studentId in chunk)
                        {
                            var examSchedule = new ExamSchedule
                            {
                                ExamId = exam.ExamId,
                                StudentId = studentId,
                                TeacherId = proctor.Id,
                                RoomId = randomRoom.RoomId,
                                SlotId = scheduleAssignment.SlotInfo.SlotId,
                                ExamDate = scheduleAssignment.SlotInfo.Date
                            };

                            _context.ExamSchedules.Add(examSchedule);
                        }

                        await _context.SaveChangesAsync();
                    }
                }


                // 8. Return success with schedule details
                return new SchedulingResult
                {
                    Success = true,
                    Message = "Exams scheduled successfully",
                    ScheduleId = createdExamIds.FirstOrDefault(), // Use first exam ID as reference
                    ScheduledExams = await GetScheduledExamInfoByExamNameAsync(sessionExamName)
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in ScheduleExamsAsync");

                return new SchedulingResult
                {
                    Success = false,
                    Message = "An error occurred during exam scheduling: " + ex.Message
                };
            }
        }


        // Modify GetAvailableProctorAsync to use courseId for direct mapping
        private async Task<AspNetUser> GetAvailableProctorAsync(DateTime examDate, int slotId)
        {
            // Find teachers who aren't already assigned to proctor exams at this time
            var busyTeacherIds = await _context.ExamSchedules
                .Where(es => es.ExamDate.Date == examDate.Date && es.SlotId == slotId)
                .Select(es => es.TeacherId)
                .Distinct()
                .ToListAsync();

            // Find a teacher who is available (not already proctoring)
            var availableTeacher = await _context.Users
                .Where(u => !busyTeacherIds.Contains(u.Id))
                .Join(_context.UserRoles,
                        u => u.Id,
                        ur => ur.UserId,
                        (u, ur) => new { User = u, RoleId = ur.RoleId })
                .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { User = ur.User, Role = r })
                .Where(x => x.Role.Name == "Teacher")
                .Select(x => x.User)
                .FirstOrDefaultAsync();

            _logger.LogInformation($"Selected proctor with Id: {availableTeacher?.Id}");

            return availableTeacher;
        }



        // Updated method to get scheduled exam info by exam name
        private async Task<List<ScheduledExamInfo>> GetScheduledExamInfoByExamNameAsync(string examName)
        {
            return await _context.Exams
                .Where(e => e.ExamName == examName)
                .Select(e => new ScheduledExamInfo
                {
                    ExamId = e.ExamId,
                    CourseName = e.Course.CourseName,
                    ExamDate = e.ExamDate,
                    SlotId = e.SlotId,
                    SlotName = e.Slot.SlotName,
                    StartTime = e.Slot.StartTime,
                    EndTime = e.Slot.EndTime,
                    RoomId = e.RoomId,
                    RoomName = e.Room.RoomName,
                    StudentCount = e.ExamSchedules.Count,
                    TeacherName = e.ExamSchedules
                                   .Select(es => es.Teacher.FirstName + " " + es.Teacher.LastName)
                                   .FirstOrDefault() ?? "Not assigned"
                })
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }


        public async Task<SchedulingResult> GetScheduleDetailsAsync(int examId)
        {
            try
            {
                // Get the exam to find the session name
                var exam = await _context.Exams.FindAsync(examId);

                if (exam == null)
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "Exam not found"
                    };
                }

                // Get all exams with the same ExamName (they belong to the same scheduling session)
                var scheduledExams = await GetScheduledExamInfoByNameAsync(exam.ExamName);

                return new SchedulingResult
                {
                    Success = true,
                    ScheduleId = examId,
                    ScheduledExams = scheduledExams
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving schedule details for ID {examId}");

                return new SchedulingResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving schedule details"
                };
            }
        }

        public async Task<List<CourseDTO>> GetCoursesAsync()
        {
            return await _context.Courses
                .Select(c => new CourseDTO
                {
                    CourseId = c.CourseId, // Fix property name inconsistency
                    CourseName = c.CourseName,
                    Description = c.Description,
                })
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        #region Helper Methods

        private async Task<Dictionary<int, List<string>>> GetEnrolledStudentsForCoursesAsync(List<int> courseIds)
        {
            var result = new Dictionary<int, List<string>>();

            foreach (var courseId in courseIds)
            {
                var studentIds = await _context.StudentClasses
                    .Where(sc => sc.Class.CourseId == courseId && sc.Status == "Enrolled")
                    .Select(sc => sc.StudentId)
                    .Distinct()
                    .ToListAsync();

                result[courseId] = studentIds;
            }

            return result;
        }

        private async Task<List<SlotWithDateInfo>> GetAvailableSlotsInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var slots = await _context.Slots.ToListAsync();
            var result = new List<SlotWithDateInfo>();

            // Generate all possible date/slot combinations in the range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                foreach (var slot in slots)
                {
                    result.Add(new SlotWithDateInfo
                    {
                        SlotId = slot.SlotId,
                        SlotName = slot.SlotName,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Date = date
                    });
                }
            }

            return result;
        }

        private async Task<List<Room>> GetAvailableRoomsAsync()
        {
            try
            {
                // Get only classrooms (RoomTypeId == 1) that are available
                var rooms = await _context.Rooms
                    .Where(r => r.RoomTypeId == 1
                                && r.Status == "Available"
                                && (r.IsAction == true || r.IsAction == null))
                    .OrderByDescending(r => r.Capacity)
                    .ToListAsync();

                _logger.LogInformation($"Available room IDs: {string.Join(", ", rooms.Select(r => r.RoomId))}");
                return rooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available rooms");
                return new List<Room>();
            }
        }

        private Dictionary<int, List<int>> BuildCourseConflictGraph(Dictionary<int, List<string>> courseStudentMap)
        {
            var graph = new Dictionary<int, List<int>>();

            // Initialize graph
            foreach (var courseId in courseStudentMap.Keys)
            {
                graph[courseId] = new List<int>();
            }

            // Create edges between courses that share students
            var courseIds = courseStudentMap.Keys.ToList();

            for (int i = 0; i < courseIds.Count; i++)
            {
                for (int j = i + 1; j < courseIds.Count; j++)
                {
                    var course1 = courseIds[i];
                    var course2 = courseIds[j];

                    var studentsInCourse1 = courseStudentMap[course1];
                    var studentsInCourse2 = courseStudentMap[course2];

                    // Check if there are common students
                    if (studentsInCourse1.Intersect(studentsInCourse2).Any())
                    {
                        // These courses have common students, so they can't be scheduled at the same time
                        graph[course1].Add(course2);
                        graph[course2].Add(course1);
                    }
                }
            }

            return graph;
        }

        private SchedulingPlan GenerateSchedulingPlan(
    List<Course> courses,
    Dictionary<int, List<int>> conflictGraph,
    Dictionary<int, List<string>> courseStudentMap,
    List<SlotWithDateInfo> availableSlots,
    List<Room> availableRooms)
        {
            var plan = new SchedulingPlan();

            _logger.LogInformation($"Available rooms for scheduling: {string.Join(", ", availableRooms.Select(r => r.RoomId))}");

            if (!availableRooms.Any())
            {
                plan.IsValid = false;
                plan.ErrorMessage = "No available rooms for scheduling";
                return plan;
            }

            // Sort courses by complexity (number of conflicts, then student count)
            var sortedCourses = courses
                .OrderByDescending(c => conflictGraph[c.CourseId].Count)
                .ThenByDescending(c => courseStudentMap[c.CourseId].Count)
                .ToList();

            var assignedSlots = new Dictionary<int, SlotWithDateInfo>();
            var usedRoomsInSlot = new Dictionary<string, HashSet<int>>();

            foreach (var course in sortedCourses)
            {
                // Get conflicts for this course
                var conflicts = conflictGraph[course.CourseId];

                // Get previously used slots by conflicting courses
                var conflictingSlotKeys = conflicts
                    .Where(assignedSlots.ContainsKey)
                    .Select(c => $"{assignedSlots[c].Date:yyyy-MM-dd}_{assignedSlots[c].SlotId}")
                    .ToHashSet();

                SlotWithDateInfo selectedSlot = null;
                Room selectedRoom = null;

                // For scheduling exam, use effective count = min(total enrollment, 15)
                int actualCount = courseStudentMap[course.CourseId].Count;
                int effectiveCount = actualCount > 15 ? 15 : actualCount;

                foreach (var slot in availableSlots)
                {
                    var slotKey = $"{slot.Date:yyyy-MM-dd}_{slot.SlotId}";

                    if (conflictingSlotKeys.Contains(slotKey))
                        continue;

                    if (!usedRoomsInSlot.ContainsKey(slotKey))
                        usedRoomsInSlot[slotKey] = new HashSet<int>();

                    foreach (var room in availableRooms)
                    {
                        if (usedRoomsInSlot[slotKey].Contains(room.RoomId))
                            continue;

                        int effectiveCapacity = Math.Min(room.Capacity, 15);
                        if (effectiveCapacity < effectiveCount)
                            continue;

                        // Suitable room found
                        selectedRoom = room;
                        selectedSlot = slot;
                        usedRoomsInSlot[slotKey].Add(room.RoomId);
                        break;
                    }

                    if (selectedRoom != null)
                        break;
                }

                if (selectedSlot == null || selectedRoom == null)
                {
                    plan.IsValid = false;
                    plan.ErrorMessage = $"Could not find an available slot and room for course {course.CourseName}";
                    return plan;
                }

                _logger.LogInformation($"Scheduled course {course.CourseId} in room {selectedRoom.RoomId} on {selectedSlot.Date.ToShortDateString()} slot {selectedSlot.SlotId}");

                plan.CourseAssignments[course.CourseId] = new CourseScheduleAssignment
                {
                    CourseId = course.CourseId,
                    SlotInfo = selectedSlot,
                    RoomId = selectedRoom.RoomId,
                    StudentIds = courseStudentMap[course.CourseId] // full enrollment; will split later
                };

                if (plan.CourseAssignments[course.CourseId].RoomAssignments == null)
                {
                    plan.CourseAssignments[course.CourseId].RoomAssignments = new Dictionary<int, List<string>>
            {
                { selectedRoom.RoomId, courseStudentMap[course.CourseId] }
            };
                }

                assignedSlots[course.CourseId] = selectedSlot;
            }

            plan.IsValid = true;
            return plan;
        }




        // New method to get scheduled exams by exam name
        private async Task<List<ScheduledExamInfo>> GetScheduledExamInfoByNameAsync(string examName)
        {
            return await _context.Exams
                .Where(e => e.ExamName == examName)
                .Select(e => new ScheduledExamInfo
                {
                    ExamId = e.ExamId,
                    CourseName = e.Course.CourseName,
                    ExamDate = e.ExamDate,
                    SlotId = e.SlotId,
                    SlotName = e.Slot.SlotName,
                    StartTime = e.Slot.StartTime,
                    EndTime = e.Slot.EndTime,
                    RoomId = e.RoomId,
                    RoomName = e.Room.RoomName,
                    StudentCount = e.ExamSchedules.Count,
                    TeacherName = e.ExamSchedules
                                   .Select(es => es.Teacher.FirstName + " " + es.Teacher.LastName)
                                   .FirstOrDefault() ?? "Not assigned"
                })
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }

        #endregion

        private static List<List<T>> Chunk<T>(List<T> source, int chunkSize)
        {
            var chunks = new List<List<T>>();
            for (int i = 0; i < source.Count; i += chunkSize)
            {
                chunks.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
            }
            return chunks;
        }

    }

    #region Helper Classes



    public class SlotWithDateInfo
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime Date { get; set; }
    }

    public class SchedulingPlan
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<int, CourseScheduleAssignment> CourseAssignments { get; set; } =
            new Dictionary<int, CourseScheduleAssignment>();
    }

    public class CourseScheduleAssignment
    {
        public int CourseId { get; set; }
        public SlotWithDateInfo SlotInfo { get; set; }
        public int RoomId { get; set; }
        public Dictionary<int, List<string>> RoomAssignments { get; set; } // RoomId -> List of StudentIds
        public List<string> StudentIds { get; set; } // All students for this course
    }



    #endregion
}
