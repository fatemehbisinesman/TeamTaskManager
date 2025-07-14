using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamTaskManager.Models;
using TeamTaskManager.Helpers;
using TeamTaskManager.Data;
using System.Collections.Generic;

namespace TeamTaskManager.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> MyProjectDetails(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            // فقط اگه کاربر عضو پروژه هست اجازه ورود داره
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!project.Members.Any(m => m.UserId == userId))
                return Forbid();

            return View(project);
        }
        // GET: Projects/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var project = _context.Projects.Find(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<Project>());
            }

            var results = _context.Projects
                .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
                .ToList();

            return View(results);
        }
        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Project updatedProject)
        {
            if (id != updatedProject.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(updatedProject);
            }

            try
            {
                var existingProject = _context.Projects.FirstOrDefault(p => p.Id == id);
                if (existingProject == null)
                    return NotFound();

                // فقط فیلدهایی که قابل ویرایش هستند را بروزرسانی کن
                existingProject.Title = updatedProject.Title;
                existingProject.Description = updatedProject.Description;
                existingProject.Price = updatedProject.Price;
                existingProject.Status = updatedProject.Status;
                existingProject.IsActive = updatedProject.IsActive;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "پروژه با موفقیت ویرایش شد.";
                return RedirectToAction("Index","Projects"); // تغییر به این خط

            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "خطا در ویرایش پروژه.";
                return View(updatedProject);
            }
        }

        [HttpGet]
        public async Task<IActionResult> StartPayment(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            // ✅ اگر پروژه رایگانه، مستقیم بهش بده
            if (project.Price == 0)
            {
                _context.UserModules.Add(new UserModule
                {
                    UserId = userId.Value,
                    ModuleName = $"Project_{project.Id}",
                    ProjectId = project.Id,
                    IsPaid = true,
                    IsProject = true
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "پروژه رایگان با موفقیت فعال شد ✔️";
                return RedirectToAction("AvailableModules", "Modules");
            }

            var zp = new ZarinpalClassic();
            string callbackUrl = Url.Action("VerifyPayment", "Projects", new { id = project.Id }, protocol: Request.Scheme);

            var result = await zp.RequestAsync(project.Price, $"پرداخت پروژه: {project.Title}", callbackUrl);

            if (result.Success)
                return Redirect("https://sandbox.zarinpal.com/pg/StartPay/" + result.Authority);

            TempData["Error"] = "خطا در اتصال به درگاه: " + result.ErrorMessage;
            return RedirectToAction("AvailableModules", "Modules");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyPayment(string authority, string status, int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || status != "OK")
                return RedirectToAction("Index", "Projects");

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            var zp = new ZarinpalClassic();
            var verifyResult = await zp.VerifyAsync(project.Price, authority);

            if (verifyResult.Success)
            {
                // ذخیره دسترسی کاربر
                _context.UserModules.Add(new UserModule
                {
                    UserId = userId.Value,
                    ModuleName = $"Project_{project.Id}",
                    ProjectId = project.Id,
                    IsPaid = true,
                    IsProject = true
                });

                // ثبت پرداخت موفق
                var user = await _context.Users.FindAsync(userId);
                _context.Payments.Add(new Payment
                {
                    UserId = userId.Value,
                    UserName = user?.FullName ?? "",
                    ProjectId = project.Id,
                    Amount = project.Price,
                    PaymentFor = $"پروژه {project.Title}",
                    Status = "پرداخت موفق",
                    RefId = verifyResult.RefId.ToString(),
                    PaymentDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ پرداخت با موفقیت انجام شد.";
                return RedirectToAction("Index", "Orders"); // ریدایرکت به سفارشات
            }
            else
            {
                TempData["Error"] = "❌ خطای تأیید پرداخت: " + verifyResult.ErrorMessage;
                return RedirectToAction("Index", "Projects");
            }
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            int? userId = HttpContext.Session.GetInt32("UserId");
            string userRole = HttpContext.Session.GetString("UserRole");

            try
            {
                var allProjects = await _context.Projects.ToListAsync();

                // 👇 جمع‌آوری عکس کاربران سازنده پروژه
                var userImages = _context.Users
                    .Where(u => allProjects.Select(p => p.CreatedBy).Contains(u.Id))
                    .ToDictionary(u => u.Id, u => u.ProfileImage ?? "/assets/img/team-1.jpg");

                ViewBag.CreatorImages = userImages;

                if (userRole == "Admin")
                {
                    ViewBag.Payments = await _context.Payments.OrderByDescending(p => p.PaymentDate).ToListAsync();
                    return View(allProjects);
                }
                else
                {
                    return View(allProjects);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ خطا در Index پروژه‌ها: " + ex.Message);
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
                return NotFound();

            var paidAmount = _context.Payments
                .Where(p => p.ProjectId == id && p.Status == "پرداخت موفق")
                .Sum(p => (int?)p.Amount) ?? 0;

            ViewBag.PaidAmount = paidAmount;

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProject(Project model)
        {
            if (!ModelState.IsValid)
                return View("Details", model); // نمایش مجدد فرم با خطاها

            var project = _context.Projects.Find(model.Id);
            if (project == null)
                return NotFound();

            project.Description = model.Description;
            project.IsActive = model.IsActive;

            _context.SaveChanges();

            return RedirectToAction("Details", new { id = model.Id });
        }



        // GET: Projects/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, IFormFile coverImage)
        {
            if (ModelState.IsValid)
            {
                // اگر عکس داده شده
                if (coverImage != null && coverImage.Length > 0)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");

                    // اگر مسیر وجود نداشت بساز
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    // آدرس برای ذخیره در دیتابیس
                    project.CoverImagePath = "/uploads/projects/" + uniqueFileName;
                }

                // اطلاعات پایه پروژه
                project.CreatedAt = DateTime.Now;
                project.Status = "در انتظار تأیید";

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(project);
        }

        [HttpGet]
        public IActionResult CreateGroupProject()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupProject(Project project, IFormFile coverImage)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                project.CreatedAt = DateTime.Now;
                project.CreatedBy = HttpContext.Session.GetInt32("UserId") ?? 0;
                project.Status = "Pending";
                project.IsPaid = true;
                project.IsActive = true;

                if (coverImage != null && coverImage.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
                    Directory.CreateDirectory(uploadsDir);

                    var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        coverImage.CopyTo(stream);
                    }

                    project.CoverImagePath = "/uploads/projects/" + fileName;
                }

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(project);
        }

        [HttpGet]
        public IActionResult CreateKanbanBoard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
                return NotFound();

            return View(project); // نمایش ویو تأیید حذف
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            // حذف فایل‌های مرتبط در صورت وجود
            if (project.Files != null && project.Files.Any())
            {
                foreach (var file in project.Files)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.ProjectFiles.RemoveRange(project.Files);
            }

            // حذف تصویر کاور (اختیاری)
            if (!string.IsNullOrEmpty(project.CoverImagePath))
            {
                var coverPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(coverPath))
                    System.IO.File.Delete(coverPath);
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "پروژه با موفقیت حذف شد.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKanbanBoard(Project project, IFormFile coverImage)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                project.CreatedAt = DateTime.Now;
                project.CreatedBy = HttpContext.Session.GetInt32("UserId") ?? 0;
                project.Status = "Pending";
                project.IsPaid = true;
                project.IsActive = true;

                if (coverImage != null && coverImage.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
                    Directory.CreateDirectory(uploadsDir);

                    var fileName = Guid.NewGuid() + Path.GetExtension(coverImage.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    project.CoverImagePath = "/uploads/projects/" + fileName;
                }

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(project);
        }

       
    }
}