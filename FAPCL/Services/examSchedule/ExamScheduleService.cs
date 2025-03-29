﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAPCL.DTO;
using FAPCL.DTO.ExamSchedule;
using FAPCL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                // 1. Validate input and check if courses exist.
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

                // 2. Get all enrolled students for these courses.
                var courseStudentMap = await GetEnrolledStudentsForCoursesAsync(courseIds);
                if (courseStudentMap.Values.Any(list => !list.Any()))
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "One or more courses have no enrolled students"
                    };
                }

                // 3. Get available time slots in the selected date range.
                var availableSlots = await GetAvailableSlotsInDateRangeAsync(startDate, endDate);
                if (!availableSlots.Any())
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "No available time slots in the selected date range"
                    };
                }

                // 4. Get available rooms (RoomTypeId == 1).
                var availableRooms = await GetAvailableRoomsAsync();
                if (!availableRooms.Any())
                {
                    return new SchedulingResult
                    {
                        Success = false,
                        Message = "No available rooms for scheduling"
                    };
                }

                // 5. Build conflict graph based on common enrolled students.
                var conflictGraph = BuildCourseConflictGraph(courseStudentMap);

                // 6. Generate the scheduling plan using the updated constraints.
                var schedulingPlan = GenerateSchedulingPlan(
                    courses,
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

                // Generate a unique session identifier for grouping these exams.
                string sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string sessionExamName = $"{examName} [Session:{sessionId}]";

                // 7. Create exams and exam schedules.
                List<int> createdExamIds = new List<int>();

                foreach (var kvp in schedulingPlan.MultipleCourseAssignments)
                {
                    int courseId = kvp.Key;
                    var assignments = kvp.Value;
                    var enrolledStudents = courseStudentMap[courseId];

                    // Split enrolled students into chunks of 15.
                    var studentChunks = Chunk(enrolledStudents, 15);
                    // It is assumed that the number of chunks equals the number of exam groups scheduled for the course.
                    for (int i = 0; i < studentChunks.Count; i++)
                    {
                        var assignment = assignments[i];
                        // Create exam record.
                        var exam = new Exam
                        {
                            ExamName = sessionExamName,
                            CourseId = courseId,
                            RoomId = assignment.RoomId,
                            SlotId = assignment.SlotInfo.SlotId,
                            ExamDate = assignment.SlotInfo.Date,
                        };
                        _context.Exams.Add(exam);
                        await _context.SaveChangesAsync(); // Save to generate ExamId.
                        createdExamIds.Add(exam.ExamId);

                        // Get an available proctor for the exam.
                        var proctor = await GetAvailableProctorAsync(assignment.SlotInfo.Date, assignment.SlotInfo.SlotId);
                        if (proctor == null)
                        {
                            await transaction.RollbackAsync();
                            return new SchedulingResult
                            {
                                Success = false,
                                Message = $"No available proctor for course {courses.FirstOrDefault(c => c.CourseId == courseId)?.CourseName} exam"
                            };
                        }

                        // Create exam schedule entries for each student in the current chunk.
                        foreach (var studentId in studentChunks[i])
                        {
                            var examSchedule = new ExamSchedule
                            {
                                ExamId = exam.ExamId,
                                StudentId = studentId,
                                TeacherId = proctor.Id,
                                RoomId = assignment.RoomId,
                                SlotId = assignment.SlotInfo.SlotId,
                                ExamDate = assignment.SlotInfo.Date
                            };
                            _context.ExamSchedules.Add(examSchedule);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");

                // 8. Return the scheduling result.
                return new SchedulingResult
                {
                    Success = true,
                    Message = "Exams scheduled successfully",
                    ScheduleId = createdExamIds.FirstOrDefault(),
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

        #region  Scheduling Plan 

        /// <summary>
        /// Generates a scheduling plan that satisfies the following constraints:
        /// 1. All exam groups for a given course must be scheduled concurrently (i.e., on the same day and in the same slot).
        /// 2. If a course’s exam scheduling for a day takes up all the classrooms (i.e., total groups equals the institution-wide room count),
        ///    then that day is reserved exclusively for that course (no other course is allowed on that day).
        /// 3. No more than two courses can have exams on the same day (unless the day is exclusive).
        /// 4. Different courses scheduled on the same day must occur in different time slots.
        /// 
        /// This version removes the conflict-graph check and only enforces that the same slot is not used by two different courses.
        /// </summary>
        private SchedulingPlan GenerateSchedulingPlan(
            List<Course> courses,
            Dictionary<int, List<string>> courseStudentMap,
            List<SlotWithDateInfo> availableSlots,
            List<Room> availableRooms)
        {
            var plan = new SchedulingPlan();

            // Group available slots by day (only the date portion).
            var slotsByDay = availableSlots
                .GroupBy(s => s.Date.Date)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.SlotId).ToList());

            // For each day-slot combination, initialize available rooms (each day/slot gets a fresh copy of all available rooms).
            var availableRoomsByDaySlot = new Dictionary<(DateTime, int), List<Room>>();
            foreach (var day in slotsByDay.Keys)
            {
                foreach (var slot in slotsByDay[day])
                {
                    availableRoomsByDaySlot[(day, slot.SlotId)] = new List<Room>(availableRooms);
                }
            }

            // Track which courses are scheduled on each day.
            var coursesScheduledPerDay = new Dictionary<DateTime, HashSet<int>>();
            // Track exclusive day assignment: if a course uses all classrooms in a slot, that day is marked exclusive for it.
            var exclusiveDay = new Dictionary<DateTime, int>();

            plan.MultipleCourseAssignments = new Dictionary<int, List<CourseScheduleAssignment>>();

            // Institution-wide room count (used for exclusivity check).
            int institutionRoomCount = availableRooms.Count;

            // Sort courses by descending enrollment (more students may be more constrained).
            var sortedCourses = courses.OrderByDescending(c => courseStudentMap[c.CourseId].Count).ToList();

            foreach (var course in sortedCourses)
            {
                var enrolled = courseStudentMap[course.CourseId];
                int totalGroupsNeeded = (int)Math.Ceiling(enrolled.Count / 15.0);
                List<CourseScheduleAssignment> assignmentsForCourse = new List<CourseScheduleAssignment>();
                bool scheduledForCourse = false;

                // Iterate candidate days in chronological order.
                foreach (var day in slotsByDay.Keys.OrderBy(d => d))
                {
                    // Skip this day if it is exclusively reserved for a different course.
                    if (exclusiveDay.ContainsKey(day) && exclusiveDay[day] != course.CourseId)
                        continue;

                    // Retrieve courses already scheduled on this day.
                    var coursesOnDay = coursesScheduledPerDay.ContainsKey(day)
                        ? coursesScheduledPerDay[day]
                        : new HashSet<int>();

                    // Enforce that (unless the day is exclusive) no more than 2 courses are scheduled on the same day.
                    if (!exclusiveDay.ContainsKey(day) && coursesOnDay.Count >= 2)
                        continue;

                    // Determine which slots on this day are already used by other courses.
                    var usedSlotsOnDay = plan.MultipleCourseAssignments
                        .SelectMany(kvp => kvp.Value)
                        .Where(a => a.SlotInfo.Date.Date == day)
                        .Select(a => a.SlotInfo.SlotId)
                        .ToHashSet();

                    // Iterate over all slots on this day that are not already used.
                    foreach (var slot in slotsByDay[day].Where(s => !usedSlotsOnDay.Contains(s.SlotId)))
                    {
                        // Check available rooms for this day-slot.
                        if (!availableRoomsByDaySlot.TryGetValue((day, slot.SlotId), out var roomsAvailable))
                            continue;

                        // To schedule all exam groups concurrently for this course,
                        // we need at least 'totalGroupsNeeded' rooms available in this slot.
                        if (roomsAvailable.Count >= totalGroupsNeeded)
                        {
                            // Reserve the necessary rooms and create an assignment for each exam group.
                            for (int i = 0; i < totalGroupsNeeded; i++)
                            {
                                var chosenRoom = roomsAvailable[0];
                                roomsAvailable.RemoveAt(0);

                                var assignment = new CourseScheduleAssignment
                                {
                                    CourseId = course.CourseId,
                                    SlotInfo = new SlotWithDateInfo
                                    {
                                        SlotId = slot.SlotId,
                                        SlotName = slot.SlotName,
                                        StartTime = slot.StartTime,
                                        EndTime = slot.EndTime,
                                        Date = day
                                    },
                                    RoomId = chosenRoom.RoomId,
                                    // Store the full enrolled student list; later, these will be split into chunks of 15.
                                    StudentIds = enrolled
                                };
                                assignmentsForCourse.Add(assignment);
                            }

                            // If this course uses up all available classrooms on this day/slot,
                            // mark the day as exclusive for that course.
                            if (totalGroupsNeeded == institutionRoomCount)
                            {
                                exclusiveDay[day] = course.CourseId;
                            }

                            // Mark this course as scheduled on the day.
                            if (!coursesScheduledPerDay.ContainsKey(day))
                                coursesScheduledPerDay[day] = new HashSet<int>();
                            coursesScheduledPerDay[day].Add(course.CourseId);

                            scheduledForCourse = true;
                            break; // Course scheduled on this day; exit the slot loop.
                        }
                    }

                    if (scheduledForCourse)
                        break; // Proceed to the next course.
                }

                if (!scheduledForCourse)
                {
                    plan.IsValid = false;
                    plan.ErrorMessage = $"Not enough slots/rooms available to schedule all exam groups concurrently for course {course.CourseName}.";
                    return plan;
                }
                else
                {
                    plan.MultipleCourseAssignments[course.CourseId] = assignmentsForCourse;
                }
            }

            plan.IsValid = true;
            return plan;
        }

        #endregion

        #region Helper Methods
        private async Task<AspNetUser> GetAvailableProctorAsync(DateTime examDate, int slotId)
        {
            var busyTeacherIds = await _context.ExamSchedules
                .Where(es => es.ExamDate.Date == examDate.Date && es.SlotId == slotId)
                .Select(es => es.TeacherId)
                .Distinct()
                .ToListAsync();

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

        public async Task<DetailedExamResult> GetScheduleDetailsAsync(int examId)
        {
            try
            {
                // Query the exam record by its unique examId and project into DetailedExamInfo.
                var examDetail = await _context.Exams
                    .Where(e => e.ExamId == examId)
                    .Select(e => new DetailedExamInfo
                    {
                        ExamId = e.ExamId,
                        ExamName = e.ExamName,
                        CourseName = e.Course.CourseName,
                        ExamDate = e.ExamDate,
                        SlotId = e.SlotId,
                        SlotName = e.Slot.SlotName,
                        StartTime = e.Slot.StartTime,
                        EndTime = e.Slot.EndTime,
                        RoomId = e.RoomId,
                        RoomName = e.Room.RoomName,
                        // Assume that all exam schedules share the same teacher (proctor).
                        Teacher = new TeacherInfo
                        {
                            TeacherId = e.ExamSchedules.Select(es => es.Teacher.Id).FirstOrDefault(),
                            TeacherName = e.ExamSchedules
                                            .Select(es => es.Teacher.FirstName + " " + es.Teacher.LastName)
                                            .FirstOrDefault() ?? "Not assigned"
                        },
                        // Build the list of students for this exam.
                        Students = e.ExamSchedules
                                    .Select(es => new StudentInfo
                                    {
                                        StudentId = es.Student.Id,
                                        StudentName = es.Student.FirstName + " " + es.Student.LastName
                                    })
                                    .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (examDetail == null)
                {
                    return new DetailedExamResult
                    {
                        Success = false,
                        Message = "Exam not found"
                    };
                }

                return new DetailedExamResult
                {
                    Success = true,
                    Message = "Exam details retrieved successfully",
                    ScheduleId = examId,
                    DetailedExam = new List<DetailedExamInfo> { examDetail }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving exam details for exam id {examId}");
                return new DetailedExamResult
                {
                    Success = false,
                    Message = "An error occurred while retrieving exam details: " + ex.Message
                };
            }
        }



        // public async Task<List<CourseDTO>> GetCoursesAsync(DateTime startDate, DateTime endDate)
        // {
        //     return await _context.Courses
        //         .Where(c => _context.StudentClasses
        //                       .Any(sc => sc.Class.CourseId == c.CourseId
        //                                  && sc.Status == "Enrolled"
        //                                  && sc.EnrollmentDate >= startDate
        //                                  && sc.EnrollmentDate <= endDate))
        //         .Select(c => new CourseDTO
        //         {
        //             CourseId = c.CourseId,
        //             CourseName = c.CourseName,
        //             Description = c.Description,
        //         })
        //         .OrderBy(c => c.CourseName)
        //         .ToListAsync();
        // }


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
            foreach (var courseId in courseStudentMap.Keys)
            {
                graph[courseId] = new List<int>();
            }
            var courseIds = courseStudentMap.Keys.ToList();
            for (int i = 0; i < courseIds.Count; i++)
            {
                for (int j = i + 1; j < courseIds.Count; j++)
                {
                    var course1 = courseIds[i];
                    var course2 = courseIds[j];
                    var studentsInCourse1 = courseStudentMap[course1];
                    var studentsInCourse2 = courseStudentMap[course2];
                    if (studentsInCourse1.Intersect(studentsInCourse2).Any())
                    {
                        graph[course1].Add(course2);
                        graph[course2].Add(course1);
                    }
                }
            }
            return graph;
        }

        private static List<List<T>> Chunk<T>(List<T> source, int chunkSize)
        {
            var chunks = new List<List<T>>();
            for (int i = 0; i < source.Count; i += chunkSize)
            {
                chunks.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
            }
            return chunks;
        }

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
        public Dictionary<int, List<CourseScheduleAssignment>> MultipleCourseAssignments { get; set; } = new Dictionary<int, List<CourseScheduleAssignment>>();
    }

    public class CourseScheduleAssignment
    {
        public int CourseId { get; set; }
        public SlotWithDateInfo SlotInfo { get; set; }
        public int RoomId { get; set; }
        // RoomAssignments is available for potential extension.
        public Dictionary<int, List<string>> RoomAssignments { get; set; }
        // Holds the enrolled students for the course (to be chunked later).
        public List<string> StudentIds { get; set; }
    }

    #endregion
}
