using Microsoft.AspNetCore.Mvc;
using TeamTaskManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System;
using System.IO;
using TeamTaskManager.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Json;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        public IActionResult UsersList()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var users = _context.Users.ToList();
            return View(users);
        }
        public IActionResult PaymentsList()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            DateTime fromDate = new DateTime(2025, 6, 4);

            var payments = _context.Payments
                .Include(p => p.Project) // اگر به اطلاعات پروژه نیاز داری
                .Include(p => p.User)    // اگر به نام کاربر نیاز داری (در صورت تعریف رابطه)
                .Where(p => p.PaymentDate >= fromDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // آمار کاربران
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalAdmins = _context.Users.Count(u => u.Role == "Admin");
            ViewBag.TotalNormalUsers = _context.Users.Count(u => u.Role != "Admin");
            ViewBag.LatestUsers = _context.Users
                .OrderByDescending(u => u.RegisterDate)
                .Take(2)
                .ToList();

            // آمار پروژه‌ها بر اساس IsActive و Status
            ViewBag.TotalProjects = _context.Projects.Count();
            ViewBag.ActiveProjects = _context.Projects.Count(p => p.IsActive);               // پروژه‌های فعال
            ViewBag.CompletedProjects = _context.Projects.Count(p => p.Status == "Completed"); // پروژه‌های کامل شده
            ViewBag.PendingProjects = _context.Projects.Count(p => !p.IsActive);             // پروژه‌های غیرفعال یا در انتظار

            ViewBag.LatestProjects = _context.Projects
                .Where(p => p.CreatedAt.HasValue)
                .OrderByDescending(p => p.CreatedAt.Value)
                .Take(3)
                .ToList();

            // آمار پرداخت‌ها
            ViewBag.TotalPayments = _context.Payments.Count(p => p.Status == "PAID");
            ViewBag.TotalPaidAmount = _context.Payments
                .Where(p => p.Status == "PAID")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            ViewBag.LatestPayments = _context.Payments
                .Where(p => p.Status == "PAID")
                .OrderByDescending(p => p.PaymentDate)
                .Take(3)
                .Include(p => p.User)
                .Include(p => p.Project)
                .ToList();

            ViewBag.RecentPaidSum = _context.Payments
                .Where(p => p.Status == "PAID")
                .OrderByDescending(p => p.PaymentDate)
                .Take(3)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            // داده‌های نمودار پرداخت‌ها
            var chartStartDate = new DateTime(2025, 6, 5); // معادل ۱۵ خرداد

            var paymentsData = _context.Payments
                .Where(p => p.Status == "PAID" && p.PaymentDate >= chartStartDate)
                .AsEnumerable()
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            string[] persianMonths = {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

            var chartLabels = new List<string>();
            var chartValues = new List<int>();

            foreach (var item in paymentsData)
            {
                chartLabels.Add($"{persianMonths[item.Month - 1]} {item.Year}");
                chartValues.Add(item.Count);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartValues = chartValues;

            return View("~/Views/Admin/Dashboard.cshtml", user);
        }



        [HttpPost]
        public IActionResult CreateGroupProject(Project project, IFormFile coverImage)
        {
            if (ModelState.IsValid)
            {
                if (coverImage != null && coverImage.Length > 0)
                {
                    var fileExtension = Path.GetExtension(coverImage.FileName);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;

                    // مسیر فیزیکی پوشه‌ی ذخیره‌سازی در wwwroot
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/img/uploads/projects");
                    Directory.CreateDirectory(uploadsPath); // اگر پوشه وجود نداشت، بساز

                    var savePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        coverImage.CopyTo(stream);
                    }

                    // مسیر مجازی عکس برای استفاده در سایت (🔴 دقت کن که / آخر حتماً باشه)
                    project.CoverImagePath = "/assets/img/uploads/projects/" + fileName;
                }

                project.CreatedAt = DateTime.Now;
                _context.Projects.Add(project);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(project);
        }
        public IActionResult PrintReceipt(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var payment = _context.Payments
                .Include(p => p.User)
                .Include(p => p.Project)
                .FirstOrDefault(p => p.Id == id);

            if (payment == null)
                return NotFound();

            return View("PrintReceipt", payment);
        }

        public async Task<IActionResult> Verify(int id, string Authority, string Status)
        {
            if (Status != "OK")
            {
                ViewBag.Message = "پرداخت توسط کاربر لغو شد.";

                var canceledPayment = await _context.Payments.FindAsync(id);
                if (canceledPayment != null)
                {
                    canceledPayment.Status = "لغو شده توسط کاربر";
                    _context.Payments.Update(canceledPayment);
                    await _context.SaveChangesAsync();
                }

                return View();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var client = new HttpClient();
            var verifyRequest = new
            {
                merchant_id = "00000000-0000-0000-0000-000000000000", // کد تستی
                amount = payment.Amount,
                authority = Authority
            };

            var response = await client.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/verify.json", verifyRequest);
            var json = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(json);

            if (result?.data?.code == 100)
            {
                ViewBag.Message = "پرداخت با موفقیت انجام شد.";
                ViewBag.RefId = result.data.ref_id;

                payment.Status = "پرداخت موفق";
                payment.PaymentDate = DateTime.Now;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.Message = "پرداخت ناموفق بود";
                payment.Status = "ناموفق";
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }

            return View(); // ⬅️ این خط هم مهمه که بعد از if-else قرار بگیره
        }


        public IActionResult LoadSection(string section)
        {
            switch (section)
            {
                case "Users":
                    var users = _context.Users.ToList();
                    return PartialView("_UsersList", users);

                case "Projects":
                    var projects = _context.Projects.ToList();
                    return PartialView("_ProjectsList", projects);

                case "Reports":
                    var reportModel = new AdminReportViewModel
                    {
                        TotalUsers = _context.Users.Count(),
                        PaidProjects = _context.Orders.Count(o => o.Status == "Paid"),
                        AllProjects = _context.Projects.Count()
                    };
                    return PartialView("_Reports", reportModel);

                case "Finance":
                    var orders = _context.Orders.ToList();
                    return PartialView("_Finance", orders);

                case "Notifications":
                    return PartialView("_Notifications");

                default:
                    return Content("موردی یافت نشد.");
            }
        }
     

        [HttpPost]
        public IActionResult UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null) return NotFound();

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder); // اطمینان از وجود پوشه

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                user.ProfileImage = "/uploads/" + fileName;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "عکس پروفایل با موفقیت ذخیره شد.";
            }

            return RedirectToAction("Dashboard");
        }


        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.Find(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.ProfileImage = model.ProfileImage;

            _context.SaveChanges();

            return RedirectToAction("UsersList");
        }




        public IActionResult ChangePassword(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult ChangePassword(int id, string newPassword)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "رمز جدید باید حداقل ۸ کاراکتر باشد.";
                return RedirectToAction("ChangePassword", new { id });
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, newPassword);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
            return RedirectToAction("UsersList");
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var projects = (user != null && user.Role == "Admin")
                ? await _context.Projects.ToListAsync()
                : await _context.Projects.Where(p => p.IsActive).ToListAsync();

            return View(projects);
        }
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        public async Task<IActionResult> UserPayments(int userId)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.UserName = user.FullName;
            return View("UserPayments", payments);
        }
      
        [HttpPost, ActionName("DeleteUser")]
        public IActionResult DeleteUserConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AccessDenied", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "کاربر با موفقیت حذف شد.";
            return RedirectToAction("UsersList");
        }
        public IActionResult CommentsList()
        {
            var comments = _context.Comments
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return View(comments);
        }

        public IActionResult ApproveComment(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                comment.IsApproved = true;
                _context.SaveChanges();
            }
            return RedirectToAction("CommentsList");
        }

        public IActionResult DeleteComment(int id)
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
            }
            return RedirectToAction("CommentsList");
        }
        public IActionResult PendingList()
        {
            var pendingComments = _context.Comments
                .Include(c => c.User)
                .Where(c => !c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View("CommentsList", pendingComments);
        }
        public IActionResult ManageTasks()
        {

            

               // گرفتن پروژه‌هایی که عنوان‌شون شامل 'گروهی' یا 'کانبان' هست
               var projects = _context.Projects
                .Where(p => p.Title.Contains("مدیریت تیم فروش") || p.Title.Contains("طراحی سایت شرکتی"))
                .Select(p => new { p.Id, p.Title })
                .ToList();

            if (!projects.Any())
            {
                ViewBag.Message = "هیچ پروژه مرتبط با 'گروهی' یا 'کانبان' موجود نیست.";
                return View(new List<TaskItem>());
            }

            var projectIds = projects.Select(p => p.Id).ToList();

            var tasks = _context.Tasks
                .Where(t => projectIds.Contains(t.ProjectId))
                .Include(t => t.Project)
                .ToList();

            return View(tasks);
        }
    }
    }

