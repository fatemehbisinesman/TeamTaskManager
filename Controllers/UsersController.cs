using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TeamTaskManager.Models;
using TeamTaskManager.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ نمایش داشبورد کاربر عادی
        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // بررسی دسترسی ماژول‌ها
            ViewBag.HasGroupProjectModule = _context.UserModules
                .Any(m => m.UserId == user.Id && m.ModuleName == "GroupProject");

            ViewBag.HasKanbanModule = _context.UserModules
                .Any(m => m.UserId == user.Id && m.ModuleName == "Kanban");

            // 👇 مجموع پرداخت‌های موفق برای این کاربر
            ViewBag.PaidAmount = _context.Payments
                .Where(p => p.UserId == user.Id && p.Status == "پرداخت موفق")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            return View("~/Views/Users/Dashboard.cshtml", user);
        }

        // GET: Users/Edit/5
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != id)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != id)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                user.FullName = updatedUser.FullName;
                user.Email = updatedUser.Email;
                // سایر فیلدهای مجاز به ویرایش

                _context.Update(user);
                _context.SaveChanges();

                TempData["Success"] = "اطلاعات با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Dashboard");
            }

            return View(updatedUser);
        }



        // ✅ فرم تغییر رمز
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // ✅ پردازش تغییر رمز
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.NewPassword);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
            return RedirectToAction("Dashboard");
        }
        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || profileImage == null || profileImage.Length == 0)
                return RedirectToAction("Dashboard");

            var user = await _context.Users.FindAsync(userId);
            var fileName = $"user_{userId}_{Path.GetFileName(profileImage.FileName)}";
            var filePath = Path.Combine("wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            user.ProfileImage = $"/uploads/{fileName}";
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // ✅ نمایش اطلاعات کاربر جاری
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View(user);
        }
    }
}
