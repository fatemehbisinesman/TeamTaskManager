using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeamTaskManager.Models;
using TeamTaskManager.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System;
using TeamTaskManager.Extensions;
using System.Collections.Generic;

namespace TeamTaskManager.Controllers
{
    [Authorize]
    public class DailyPlanItemsController : Controller
    {
        private readonly AppDbContext _context;

        public DailyPlanItemsController(AppDbContext context)
        {
            _context = context;
        }

        // لیست برنامه‌های کاربر
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var plans = await _context.DailyPlanItems
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.StartTime)
                .ToListAsync();

            return View(plans);
        }

       
        // GET: DailyPlan/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyPlanItem plan, string PersianDate, string activeTab)
        {
            ModelState.Remove("activeTab");
            if (string.IsNullOrWhiteSpace(PersianDate))
            {
                ModelState.AddModelError("Date", "تاریخ برنامه‌ریزی الزامی است");

                if (string.IsNullOrEmpty(plan.UserId))
                    plan.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return View(plan);
            }

            try
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

                    var pc = new PersianCalendar();
                    plan.Date = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                }
                else
                {
                    ModelState.AddModelError("Date", "فرمت تاریخ نادرست است.");
                    return View(plan);
                }
            }
            catch
            {
                ModelState.AddModelError("Date", "تاریخ واردشده معتبر نیست.");
                return View(plan);
            }

            // اطمینان از مقداردهی کاربر
            if (string.IsNullOrEmpty(plan.UserId))
                plan.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (string.IsNullOrEmpty(plan.UserId))
            {
                TempData["FormError"] = "⚠️ خطا: کاربر شناسایی نشد.";
                return View(plan);
            }

            if (ModelState.IsValid)
            {
                _context.Add(plan);
                await _context.SaveChangesAsync();

                // مقدار پیش‌فرض اگر activeTab خالی بود
                activeTab ??= "Saturday";
                return RedirectToAction(nameof(Index), new { activeTab });
            }

            TempData["FormError"] = string.Join(" | ", ModelState
                .Where(ms => ms.Value.Errors.Any())
                .Select(ms => $"{ms.Key}: {ms.Value.Errors.First().ErrorMessage}"));

            return View(plan);
        }
        [HttpPost]
        public IActionResult BulkDelete(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["FormError"] = "هیچ برنامه‌ای انتخاب نشده است.";
                return RedirectToAction("Index");
            }

            var items = _context.DailyPlanItems.Where(p => selectedIds.Contains(p.Id)).ToList();
            _context.DailyPlanItems.RemoveRange(items);
            _context.SaveChanges();

            TempData["FormSuccess"] = "برنامه‌های انتخاب‌شده حذف شدند.";
            return RedirectToAction("Index");
        }

        // GET: DailyPlan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var plan = await _context.DailyPlanItems
                .FirstOrDefaultAsync(m => m.Id == id);

            if (plan == null) return NotFound();

            return View(plan);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.DailyPlanItems.FindAsync(id);
            if (item == null) return NotFound();

            // تبدیل تاریخ میلادی به شمسی
            if (item.Date > new DateTime(1900, 1, 1))
            {
                var pc = new PersianCalendar();
                ViewBag.PersianDate = $"{pc.GetYear(item.Date)}/{pc.GetMonth(item.Date):00}/{pc.GetDayOfMonth(item.Date):00}";
            }
            else
            {
                ViewBag.PersianDate = "";
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DailyPlanItem plan, string PersianDate)
        {
            Console.WriteLine("🚀 وارد متد POST Edit شد");
            Console.WriteLine("🆔 plan.Id = " + plan.Id);
            Console.WriteLine("📅 PersianDate = " + PersianDate);

            if (id != plan.Id)
                return NotFound();

            // تبدیل تاریخ شمسی به میلادی
            try
            {
                var persianDigits = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
                for (int j = 0; j < 10; j++)
                    PersianDate = PersianDate.Replace(persianDigits[j], j.ToString()[0]);

                var parts = PersianDate.Split('/');
                if (parts.Length == 3)
                {
                    int year = int.Parse(parts[0]);
                    int month = int.Parse(parts[1]);
                    int day = int.Parse(parts[2]);

                    var pc = new PersianCalendar();
                    plan.Date = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                }
                else
                {
                    ModelState.AddModelError("Date", "فرمت تاریخ نادرست است.");
                }
            }
            catch
            {
                ModelState.AddModelError("Date", "تاریخ واردشده معتبر نیست.");
            }

            plan.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            Console.WriteLine("📋 ModelState.IsValid = " + ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine("❌ خطا: " + error.ErrorMessage);

                ViewBag.PersianDate = PersianDate;
                return View(plan);
            }

            try
            {
                _context.Update(plan);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ ذخیره موفق!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("💥 خطا در ذخیره: " + ex.Message);
                throw;
            }
        }



        // POST: DailyPlan/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.DailyPlanItems.FindAsync(id);
            if (plan != null)
            {
                _context.DailyPlanItems.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.DailyPlanItems
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (item == null) return NotFound();

            // تبدیل تاریخ به شمسی
            var pc = new PersianCalendar();
            ViewBag.PersianDate = $"{pc.GetYear(item.Date)}/{pc.GetMonth(item.Date):00}/{pc.GetDayOfMonth(item.Date):00}";

            return View(item);
        }


    }
}
