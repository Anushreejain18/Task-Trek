using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LoginApp.Models;

namespace LoginApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly StudentDBContext _context = new StudentDBContext();

        // Teacher Login
        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (username == "teacher" && password == "123456")
            {
                Session["UserRole"] = "Teacher";
                Session["Username"] = username;
                return RedirectToAction("TaskRecord");
            }
            ViewBag.Error = "Invalid login!";
            return View();
        }
        // Student Login
        public ActionResult StudentLog() => View();

        [HttpPost]
        public async Task<ActionResult> StudentLog(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null)
            {
                ViewBag.Error = "Username not found.";
                return View();
            }

            if (student.Password != password)
            {
                ViewBag.Error = "Invalid password.";
                return View();
            }

            // ✅ Store additional session data after successful login
            Session["UserRole"] = "Student";
            Session["Username"] = username;
            Session["StudentId"] = student.Id;
            Session["StudentName"] = student.Name;
            Session["StudentClass"] = student.Class;

            return RedirectToAction("StudentTasks"); // or your intended landing page
        }

        //AddStudent form
        public ActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddStudent(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.Name) ||
                string.IsNullOrWhiteSpace(student.RollNo) ||
                string.IsNullOrWhiteSpace(student.Class) ||
                string.IsNullOrWhiteSpace(student.Username) ||
                string.IsNullOrWhiteSpace(student.Password) ||
                string.IsNullOrWhiteSpace(student.Email) ||
                string.IsNullOrWhiteSpace(student.Phone) ||
                student.DateOfBirth == null)
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("AddStudent");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Student added successfully!";
            return RedirectToAction("AddStudent");
        }

        // View list of students with pagination and class filtering
        public async Task<ActionResult> StudentList(string selectedClass, int page = 1, int pageSize = 10)
        {
            try
            {
                // Fetch distinct classes for dropdown
                ViewBag.Classes = await _context.Students
                                        .Where(s => s.Class != null)
                                        .Select(s => s.Class)
                                        .Distinct()
                                        .OrderBy(s => s) // Optional: Sort classes alphabetically
                                        .ToListAsync();

                var studentsQuery = _context.Students.AsQueryable();

                // Apply class filter if a class is selected
                if (!string.IsNullOrEmpty(selectedClass))
                {
                    studentsQuery = studentsQuery.Where(s => s.Class == selectedClass);
                }

                // Get total records after filtering
                int totalRecords = await studentsQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // Fetch students based on pagination
                var students = await studentsQuery
                    .OrderBy(s => s.Name) // Sort students (optional)
                    .Skip((page - 1) * pageSize) // Skip records for previous pages
                    .Take(pageSize) // Get only required records
                    .ToListAsync();

                // Handle empty results
                if (!students.Any())
                {
                    TempData["Message"] = "No students found.";
                }

                // Pass necessary data to the view
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SelectedClass = selectedClass; // Preserve selected class

                return View(students);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while fetching student data.";
                return View(new List<Student>());
            }
        }


        // Edit Student (GET)
        public async Task<ActionResult> EditStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                ViewBag.Error = "Student not found.";
                return View();
            }

            ViewBag.StudentId = student.Id;
            ViewBag.Name = student.Name;
            ViewBag.RollNo = student.RollNo;
            ViewBag.Class = student.Class;
            ViewBag.Email = student.Email;
            ViewBag.Phone = student.Phone;
            ViewBag.DateOfBirth = student.DateOfBirth.ToString("yyyy-MM-dd"); // date format for input[type=date]
            ViewBag.Username = student.Username;
            ViewBag.Password = student.Password;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditStudent(int StudentId, string Name, string RollNo, string Class, string Username, string Password, string Email, string Phone, DateTime DateOfBirth)
        {
            var student = await _context.Students.FindAsync(StudentId);
            if (student == null)
            {
                ViewBag.Error = "Student not found.";
                return View();
            }

            student.Name = Name;
            student.RollNo = RollNo;
            student.Class = Class;
            student.Username = Username;
            student.Password = Password;
            student.Email = Email;
            student.Phone = Phone;
            student.DateOfBirth = DateOfBirth;

            await _context.SaveChangesAsync();
            TempData["Message"] = "Student updated successfully!";
            return RedirectToAction("StudentList");
        }


        [HttpPost]
        public async Task<ActionResult> DeleteStudentConfirmed(int id)
        {
            if (id == 0)
            {
                TempData["Error"] = "Invalid student ID.";
                return RedirectToAction("StudentList");
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("StudentList");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Student deleted successfully!";
            return RedirectToAction("StudentList");
        }


        // Add Task
        public async Task<ActionResult> AddTasks()
        {
            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Classes = await _context.Students.Select(s => s.Class).Distinct().ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddTasks(StudentTask task, int[] assignedStudents)
        {
            if (!ModelState.IsValid || assignedStudents == null || !assignedStudents.Any())
            {
                ViewBag.Error = "All fields are required.";
                ViewBag.Classes = await _context.Students.Select(s => s.Class).Distinct().ToListAsync();
                ViewBag.Students = await _context.Students.ToListAsync();
                return View(task);
            }

            if (task.StartDate > task.EndDate)
            {
                ViewBag.Error = "Start date cannot be after end date.";
                ViewBag.Classes = await _context.Students.Select(s => s.Class).Distinct().ToListAsync();
                ViewBag.Students = await _context.Students.ToListAsync();
                return View(task);
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var assignedStudentDetails = await _context.Students
                .Where(s => assignedStudents.Contains(s.Id))
                .ToListAsync();

            var taskRecords = assignedStudentDetails.Select(student => new TaskRecord
            {
                Title = task.Title,
                Description = task.Description,
                StudentName = student.Name,
                Class = student.Class,
                RollNumber = student.RollNo,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Status = "Pending",
                Reply = "",
                TaskId = task.TaskId
            }).ToList();

            _context.TaskRecords.AddRange(taskRecords);
            await _context.SaveChangesAsync();

            // Add notifications for assigned students
            foreach (var student in assignedStudentDetails)
            {
                await AddNotification(student.Username, "Student", $"New task assigned: {task.Title}", task.TaskId);
            }
            TempData["Message"] = "Task added successfully!";
            return RedirectToAction("AddTasks");
        }

        // GET: Task/GetStudentsByClass
        [HttpGet]
        public async Task<JsonResult> GetStudentsByClass(string selectedClass)
        {
            if (string.IsNullOrEmpty(selectedClass))
            {
                return Json(new { error = "Class is required." }, JsonRequestBehavior.AllowGet);
            }

            var students = await _context.Students
                .Where(s => s.Class == selectedClass)
                .Select(s => new { id = s.Id, name = s.Name, rollNo = s.RollNo })
                .ToListAsync();

            return Json(students, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> TaskRecord(string searchTerm, string selectedClass = "all", int page = 1, int pageSize = 10)
        {
            var taskRecords = _context.TaskRecords
                .Include(tr => tr.Task)
                .AsQueryable();

            // 🔍 Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower(); // Case-insensitive search
                taskRecords = taskRecords.Where(tr =>
                    (tr.Title ?? "").ToLower().Contains(searchTerm) ||
                    (tr.StudentName ?? "").ToLower().Contains(searchTerm) ||
                    (tr.Status ?? "").ToLower().Contains(searchTerm) ||
                    (tr.RollNumber != null && tr.RollNumber.ToString().Contains(searchTerm))
                );
            }

            // 🔍 Apply class filter
            if (!string.IsNullOrWhiteSpace(selectedClass) && selectedClass.ToLower() != "all")
            {
                taskRecords = taskRecords.Where(tr => tr.Class.ToLower() == selectedClass.ToLower());
            }

            // ✅ Get total count for pagination
            int totalRecords = await taskRecords.CountAsync();

            // ✅ Apply pagination
            var result = await taskRecords
                .OrderBy(tr => tr.TaskRecordId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Get distinct class names for dropdown
            ViewBag.Classes = await _context.TaskRecords
                                    .Select(tr => tr.Class)
                                    .Distinct()
                                    .ToListAsync();

            // ✅ Set pagination details
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedClass = selectedClass;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // ✅ Handle AJAX request (return only table)
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TaskRecordTable", result);
            }

            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> TaskRecordAjax(string searchTerm)
        {
            var taskRecords = _context.TaskRecords.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                taskRecords = taskRecords.Where(tr =>
                    (tr.Title ?? "").ToLower().Contains(searchTerm) ||
                    (tr.StudentName ?? "").ToLower().Contains(searchTerm) ||
                    (tr.Status ?? "").ToLower().Contains(searchTerm) ||
                    (tr.RollNumber != null && tr.RollNumber.ToString().Contains(searchTerm))
                );
            }

            var result = await taskRecords
                .OrderBy(tr => tr.TaskRecordId)
                .ToListAsync();

            return PartialView("_TaskRecordTable", result); // Ensure this partial view exists
        }

        // GET: StudentTasks
        public async Task<ActionResult> StudentTasks(int? selectedTaskId)
        {
            var username = Session["Username"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("LoginPage", "Home");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found. Please log in again.";
                return RedirectToAction("Login");
            }

            var studentTasks = await _context.TaskRecords
                .Where(t => t.StudentName == student.Name)
                .ToListAsync();

            ViewBag.StudentTasks = studentTasks;

            // Ensure that a selected task is properly assigned
            if (selectedTaskId.HasValue)
            {
                ViewBag.SelectedTask = studentTasks.FirstOrDefault(t => t.TaskRecordId == selectedTaskId.Value);
            }

            return View();
        }


        // POST: StudentTasks
        [HttpPost]
        public async Task<ActionResult> StudentTasks(
         int taskIds,
         string statuses,
         string replies,
         HttpPostedFileBase files)
        {
            var username = Session["Username"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found. Please log in again.";
                return RedirectToAction("Login");
            }

            var taskRecord = await _context.TaskRecords
                .Where(tr => tr.TaskRecordId == taskIds && tr.StudentName == student.Name)
                .FirstOrDefaultAsync();

            if (taskRecord != null)
            {
                taskRecord.Status = statuses;
                taskRecord.Reply = replies;

                // Handle file upload only if status is "Completed"
                if (statuses == "Completed" && files != null && files.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(files.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                    // Ensure the directory exists
                    Directory.CreateDirectory(Server.MapPath("~/Uploads"));

                    // Save the file
                    files.SaveAs(filePath);

                    // Store relative path in database
                    taskRecord.FilePath = "/Uploads/" + fileName;
                }

                _context.Entry(taskRecord).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Notify the teacher about the update
                await AddNotification("teacher", "Teacher", $"Task updated by {taskRecord.StudentName}: {taskRecord.Title}", taskRecord.TaskId);

                TempData["SuccessMessage"] = "Task updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Task not found or unauthorized access.";
            }

            return RedirectToAction("StudentTasks", new { selectedTaskId = taskIds });
        }
        //Logout
        public ActionResult Logout()
        {

            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["AuthCookie"] != null)
            {
                var cookie = new HttpCookie("AuthCookie")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = null
                };
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index", "Home");
        }
        // Forgot Password Page
        public ActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string username)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null)
            {
                ViewBag.Error = "No student found with that username.";
                return View();
            }

            Session["ResetUsername"] = username;
            return RedirectToAction("ResetPassword");
        }

        // Reset Password Page
        public ActionResult ResetPassword()
        {
            var username = Session["ResetUsername"]?.ToString();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("ForgotPassword");

            ViewBag.Username = username;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string newPassword)
        {
            var username = Session["ResetUsername"]?.ToString();
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("ForgotPassword");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                return View();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null)
            {
                ViewBag.Error = "An error occurred. Please try again.";
                return View();
            }

            student.Password = newPassword;
            await _context.SaveChangesAsync();


            Session.Remove("ResetUsername");

            TempData["Success"] = "Password reset successfully! Please log in.";
            return RedirectToAction("StudentLog");
        }
        // Add notification
        private async Task AddNotification(string username, string userRole, string message, int? taskId = null)
        {
            var notification = new Notification
            {
                Username = username,
                UserRole = userRole,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now,
                TaskId = taskId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // View unread notifications with pagination
        public async Task<ActionResult> Notifications(int page = 1, int pageSize = 5)
        {
            var username = Session["Username"]?.ToString();
            var userRole = Session["UserRole"]?.ToString();

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            // Fetch only unread notifications
            var query = _context.Notifications
                .Where(n => n.Username == username && n.UserRole == userRole && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt);

            int totalNotifications = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.NotificationCount = totalNotifications;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalNotifications / pageSize);

            return View(notifications);
        }

        // Mark notification as read
        [HttpPost]
        public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Notifications");
        }

        // Fetch unread notification count (AJAX support)
        [HttpGet]
        public async Task<JsonResult> GetUnreadNotificationCount()
        {
            var username = Session["Username"]?.ToString();
            var userRole = Session["UserRole"]?.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userRole))
            {
                return Json(new { count = 0, userRole = "" }, JsonRequestBehavior.AllowGet);
            }

            var unreadCount = await _context.Notifications
                .Where(n => n.Username == username && !n.IsRead)
                .CountAsync();

            return Json(new { count = unreadCount, userRole = userRole }, JsonRequestBehavior.AllowGet);
        }
        // Teacher Dashboard
        public async Task<ActionResult> Dashboard(string selectedClass)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login");

            // Fetch all unique classes from TaskRecords
            var classList = await _context.TaskRecords
                .Where(tr => !string.IsNullOrEmpty(tr.Class))
                .Select(tr => tr.Class)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Classes = classList;

            // If no class selected, auto-select the first one
            if (string.IsNullOrEmpty(selectedClass) && classList.Any())
            {
                selectedClass = classList.First();
                return RedirectToAction("Dashboard", new { selectedClass });
            }

            // If still null (e.g., no classes at all), return empty dashboard
            if (string.IsNullOrEmpty(selectedClass))
            {
                ViewBag.SelectedClass = null;
                ViewBag.TotalStudents = 0;
                ViewBag.TotalTasks = 0;
                ViewBag.PendingTasks = 0;
                ViewBag.CompletedTasks = 0;
                ViewBag.Percentage = 0;
                ViewBag.Notifications = new List<TaskRecord>();
                return View();
            }

            // Filter TaskRecords for selected class
            var records = await _context.TaskRecords
                                    .Where(tr => tr.Class == selectedClass)
                                    .ToListAsync();

            var totalTasks = records.Count;
            var pendingTasks = records.Count(tr => tr.Status == "Pending");
            var completedTasks = records.Count(tr => tr.Status == "Completed");

            // Unique students based on roll number
            var totalStudents = records
                                .Select(tr => tr.RollNumber)
                                .Distinct()
                                .Count();

            // Notifications - recent replies or completions
            var notifications = records
                .Where(tr => !string.IsNullOrEmpty(tr.Reply) || tr.Status == "Completed")
                .OrderByDescending(tr => tr.EndDate)
                .Take(5)
                .ToList();

            double percentage = totalTasks > 0
                ? (double)completedTasks / totalTasks * 100
                : 0;

            // Upcoming tasks (next 3 days)
            var upcomingTasks = records
                .Where(tr => tr.EndDate >= DateTime.Today && tr.EndDate <= DateTime.Today.AddDays(3))
                .ToList();

            // Top performing students (students with all completed tasks)
            var topStudents = records
                .GroupBy(tr => tr.StudentName)
                .Where(g => g.All(tr => tr.Status == "Completed") && g.Count() > 0)
                .Select(g => g.Key)
                .ToList();

            // Pass data to view
            ViewBag.SelectedClass = selectedClass;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.PendingTasks = pendingTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.Percentage = percentage;
            ViewBag.Notifications = notifications;
            ViewBag.UpcomingTasks = upcomingTasks;
            ViewBag.TopStudents = topStudents;

            return View();
        }

        // GET: /Account/StudentProfile
        public async Task<ActionResult> StudentProfile()
        {
            if (Session["UserRole"]?.ToString() != "Student" || Session["Username"] == null)
            {
                return RedirectToAction("StudentLog");
            }

            string username = Session["Username"].ToString();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);

            if (student == null)
            {
                TempData["Error"] = "Profile not found.";
                return RedirectToAction("StudentTasks");
            }

            ViewBag.ClassList = new SelectList(new[] {
        "Computer Science", "Information Science", "Electronics & Communication",
        "Mechatronics", "Artificial Intelligence & ML", "CS IoT",
        "Mechanical Engineering", "Aeronautical Engineering", "Civil Engineering"
    }, student.Class);  // Pre-select student's class

            return View(student);
        }

        // POST: /Account/UpdateStudentProfile
        [HttpPost]
        public async Task<ActionResult> UpdateStudentProfile(Student updatedStudent)
        {
            if (Session["UserRole"]?.ToString() != "Student" || Session["Username"] == null)
            {
                return RedirectToAction("StudentLog");
            }

            var student = await _context.Students.FindAsync(updatedStudent.Id);
            if (student == null)
            {
                TempData["Error"] = "Update failed. Student not found.";
                return RedirectToAction("StudentProfile");
            }

            student.Name = updatedStudent.Name;
            student.Email = updatedStudent.Email;
            student.Class = updatedStudent.Class;
            student.Phone = updatedStudent.Phone;
            student.DateOfBirth = updatedStudent.DateOfBirth;
            student.RollNo = updatedStudent.RollNo;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("StudentProfile");
        }

        public async Task<ActionResult> TaskHistory()
        {
            var username = Session["Username"]?.ToString();

            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("StudentLog");
            }

            // Fetch only tasks assigned to the current student
            var taskRecords = await _context.TaskRecords
                .Where(t => t.StudentName == username)
                .OrderByDescending(t => t.EndDate)
                .ToListAsync();

            return View(taskRecords);
        }
        // GET: Teacher Upload Resource
        public ActionResult UploadResource()
        {
            return View();
        }

        // POST: Upload Resource
        [HttpPost]
        public async Task<ActionResult> UploadResource(Resource model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                    var directoryPath = Server.MapPath("~/Uploads");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    file.SaveAs(path);
                    model.FilePath = "~/Uploads/" + fileName;
                }

                // Additional fields
                model.UploadDate = DateTime.Now;
                model.UploadedBy = "Teacher";

                // Save resource to database
                _context.Resources.Add(model);
                await _context.SaveChangesAsync();

                // ✅ Notify students in the same class
                var students = _context.Students.Where(s => s.Class == model.Class).ToList();

                foreach (var student in students)
                {
                    var notification = new Notification
                    {
                        Message = $"📘 New resource uploaded: {model.Title}",
                        CreatedAt = DateTime.Now,
                        UserRole = "Student",
                        Username = student.Username,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Resource uploaded successfully!";
                return RedirectToAction("UploadResource");
            }

            return View(model);
        }

        // GET: Student Resource View
        public async Task<ActionResult> StudentResourceView()
        {
            var username = Session["Username"]?.ToString();

            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Login expired.";
                return RedirectToAction("StudentLog", "Account");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
            if (student == null) return HttpNotFound();

            var classResources = await _context.Resources
                .Where(r => r.Class == student.Class)
                .OrderByDescending(r => r.UploadDate)  // Ensure resources are ordered by date
                .ToListAsync();

            // Check if no resources are found for this class
            if (classResources.Count == 0)
            {
                TempData["NoResources"] = "No resources found for your class.";
            }

            return View(classResources);
        }
        [HttpPost]
        public async Task<ActionResult> AddResourceComment(int resourceId, string comment)
        {
            var username = Session["Username"]?.ToString();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);

            if (student != null && !string.IsNullOrWhiteSpace(comment))
            {
                string fullComment = $"Name: {student.Name}, RollNo: {student.RollNo}, Class: {student.Class}, Comment: {comment}";
                string notificationMessage = $"🗨️ {student.Name} (RollNo: {student.RollNo}) commented on a resource.";

                if (resourceId > 0)
                {
                    // Comment on a specific resource
                    var resource = await _context.Resources.FindAsync(resourceId);
                    if (resource != null)
                    {
                        resource.StudentComment = fullComment;
                        await _context.SaveChangesAsync();

                        // Send notification to teacher
                        var notification = new Notification
                        {
                            Message = notificationMessage,
                            CreatedAt = DateTime.Now,
                            UserRole = "Teacher",
                            Username = "teacher", // Replace with actual teacher username if needed
                            IsRead = false
                        };

                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();

                        TempData["Message"] = "Your comment has been successfully added to the resource.";
                    }
                }
                else
                {
                    // General comment not tied to any specific resource
                    var generalResource = new Resource
                    {
                        Title = "General Comment",
                        Message = "General student comment",
                        UploadDate = DateTime.Now,
                        StudentComment = fullComment,
                        FilePath = null
                    };

                    _context.Resources.Add(generalResource);

                    // Send notification to teacher
                    var notification = new Notification
                    {
                        Message = notificationMessage,
                        CreatedAt = DateTime.Now,
                        UserRole = "Teacher",
                        Username = "teacher",
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);

                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Your comment has been successfully submitted.";
                }
            }

            return RedirectToAction("StudentResourceView");
        }


        // GET: Teacher View Comments
        public ActionResult ViewComments()
        {
            var comments = _context.Resources
                .Where(r => r.StudentComment != null)
                .OrderByDescending(r => r.UploadDate)
                .ToList();

            return View(comments);
        }
    }
}

