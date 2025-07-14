using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using TeamTaskManager.Models;
using TeamTaskManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult DeleteProjectOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var order = _context.UserModules.FirstOrDefault(x => x.Id == id && x.UserId == userId && x.IsProject);
            if (order != null)
            {
                _context.UserModules.Remove(order);
                _context.SaveChanges();
                TempData["Success"] = "سفارش با موفقیت حذف شد.";
            }
            else
            {
                TempData["Error"] = "سفارشی یافت نشد یا مجاز به حذف آن نیستید.";
            }

            return RedirectToAction("Index");
        }



        public IActionResult EnterProject(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var userModule = _context.UserModules
                .Include(um => um.Project)
                .FirstOrDefault(um => um.UserId == userId && um.ProjectId == id && um.IsProject);

            if (userModule == null || userModule.Project == null)
            {
                TempData["Error"] = "پروژه مورد نظر یافت نشد.";
                return RedirectToAction("Index");
            }

            var title = userModule.Project.Title?.Trim();

            if (string.IsNullOrEmpty(title))
            {
                TempData["Error"] = "عنوان پروژه نامعتبر است.";
                return RedirectToAction("Index");
            }

            // مسیر‌دهی بر اساس عنوان پروژه
            switch (title)
            {
                case "طراحی سایت شرکتی":
                case "برنامه ریزی روزانه":
                    return RedirectToAction("Dashboard", "DailyProjects", new { id });

                case "مدیریت تیم فروش":
                    return RedirectToAction("Dashboard", "GroupProjects", new { id });

                default:
                    // اگر عنوان پروژه در هیچ‌کدام نبود، هدایت به صفحه جزئیات عمومی پروژه
                    return RedirectToAction("Details", "Projects", new { id });
            }
        }







        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            // لیست پروژه‌های فعال و پرداخت‌شده
            var validProjectIds = _context.Projects
                .Where(p => p.IsActive && p.IsPaid)
                .Select(p => p.Id)
                .ToList();

            // دریافت ماژول‌های کاربر به‌همراه پروژه (در صورت وجود)
            var modules = _context.UserModules
                .Include(x => x.Project)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PurchasedAt)
                .ToList();

            // فیلتر نهایی: ماژول ثابت یا پروژه معتبر
            var validModules = modules
                .Where(x => !x.IsProject || (x.ProjectId.HasValue && validProjectIds.Contains(x.ProjectId.Value)))
                .ToList();

            return View(validModules);
        }

      

    }
}
