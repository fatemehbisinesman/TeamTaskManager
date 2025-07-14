using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models;
using Microsoft.AspNetCore.Http;
using TeamTaskManager.Data;
using TaskStatusEnum = TeamTaskManager.Models.TaskStatus;
using System.IO;

namespace TeamTaskManager.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly AppDbContext _context;

        public TaskItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToAction("Login", "Auth");

            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.AssignedUserId == currentUserId.Value)
                .ToListAsync();

            return View(tasks);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupProject(Project model, IFormFile? coverImage)
        {
            if (ModelState.IsValid)
            {
                // ذخیره تصویر (در صورت وجود)
                if (coverImage != null && coverImage.Length > 0)
                {
                    var fileName = Path.GetFileName(coverImage.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                    using var stream = new FileStream(savePath, FileMode.Create);
                    await coverImage.CopyToAsync(stream);
                    model.CoverImagePath = "/uploads/" + fileName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                // بعد از ذخیره پروژه، هدایت به صفحه ایجاد وظیفه برای همین پروژه
                return RedirectToAction("Create", "TaskItems", new { projectId = model.Id });
            }
            return View(model);
        }


        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create?projectId=xxx
        public IActionResult Create(int? projectId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            if (projectId == null)
                return Content("شناسه پروژه ارسال نشده!");

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            var currentUser = _context.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => new { u.Id, u.FullName })
                .FirstOrDefault();

            if (currentUser == null)
                return RedirectToAction("Login", "Auth");

            // فقط کاربر جاری در لیست انتخاب باشد
            ViewBag.ProjectUsers = new SelectList(new[] { currentUser }, "Id", "FullName");

            var model = new TaskItem
            {
                ProjectId = projectId.Value,
                CreatedAt = DateTime.Now
            };

            return View(model);
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Status,ProjectId,AssignedUserId")] TaskItem taskItem, string PersianDate)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            // تبدیل تاریخ شمسی به میلادی
            if (!string.IsNullOrWhiteSpace(PersianDate))
            {
                var persianDigits = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
                for (int i = 0; i < 10; i++)
                    PersianDate = PersianDate.Replace(persianDigits[i], i.ToString()[0]);

                var parts = PersianDate.Split('/');
                if (parts.Length == 3)
                {
                    int year = int.Parse(parts[0]);
                    int month = int.Parse(parts[1]);
                    int day = int.Parse(parts[2]);

                    var pc = new System.Globalization.PersianCalendar();
                    taskItem.CreatedAt = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                }
                else
                {
                    ViewBag.PersianDateError = "فرمت تاریخ نادرست است.";
                    ViewBag.ProjectUsers = new SelectList(new[] {
                _context.Users.FirstOrDefault(u => u.Id == taskItem.AssignedUserId)
            }, "Id", "FullName");
                    return View(taskItem);
                }
            }
            else
            {
                ViewBag.PersianDateError = "تاریخ ایجاد الزامی است.";
                ViewBag.ProjectUsers = new SelectList(new[] {
            _context.Users.FirstOrDefault(u => u.Id == taskItem.AssignedUserId)
        }, "Id", "FullName");
                return View(taskItem);
            }

            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("ProjectTasks", new { projectId = taskItem.ProjectId });
            }

            ViewBag.ProjectUsers = new SelectList(new[] {
        _context.Users.FirstOrDefault(u => u.Id == taskItem.AssignedUserId)
    }, "Id", "FullName");

            return View(taskItem);
        }

        public IActionResult MyTasks()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
                return RedirectToAction("Login", "Account");

            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("شناسه کاربر نامعتبر است.");

            // گرفتن پروژه‌هایی که کاربر عضو آنهاست
            var userProjectIds = _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.ProjectId)
                .ToList();

            // گرفتن وظایف مرتبط با پروژه‌های کاربر
            var tasks = _context.TaskItems
                .Where(t => userProjectIds.Contains(t.ProjectId))
                .OrderBy(t => t.Deadline)
                .ToList();

            return View(tasks);
        }
        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Status,Deadline,Priority,ProjectId")] TaskItem taskItem)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            return View(taskItem);
        }
        // GET: Projects/ManageMembers/5
        public IActionResult ManageMembers(int projectId)
        {
            ViewBag.ProjectId = projectId;

            // همه کاربران را برای انتخاب لیست کن
            var allUsers = _context.Users
                .Select(u => new { u.Id, u.FullName })
                .ToList();

            ViewBag.AllUsers = new SelectList(allUsers, "Id", "FullName");

            // اعضای فعلی پروژه
            var members = _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .ToList();

            return View(members);
        }


        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var taskItem = await _context.Tasks.FindAsync(id);
            _context.Tasks.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult ChangeStatus(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
                return NotFound();

            if (task.Status == TaskStatusEnum.Doing)
                task.Status = TaskStatusEnum.Completed;
            else if (task.Status == TaskStatusEnum.Completed)
                task.Status = TaskStatusEnum.Canceled;
            else
                task.Status = TaskStatusEnum.Doing;

            _context.SaveChanges();

            return RedirectToAction("ProjectTasks", new { projectId = task.ProjectId });

        }
        private bool TaskItemExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
        public IActionResult ProjectTasks(int projectId)
        {
            var tasks = _context.Tasks
                        .Where(t => t.ProjectId == projectId)
                        .ToList();

            return View("Index", tasks); // اگر از ویوی Index استفاده می‌کنی
        }
        public IActionResult CreateGroupProject()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToAction("Login", "Auth");

            var currentUser = _context.Users
                .Where(u => u.Id == currentUserId.Value)
                .Select(u => new { u.Id, u.FullName })
                .FirstOrDefault();

            if (currentUser == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserName = currentUser.FullName;

            return View();
        }
    }
}
