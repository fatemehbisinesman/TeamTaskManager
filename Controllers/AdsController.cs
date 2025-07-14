using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class AdsController : Controller
    {
        private readonly AppDbContext _context;

        public AdsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(role) && role == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View(await _context.Ads.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var ad = await _context.Ads.FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ad ad, IFormFile ImageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var relativePath = Path.Combine("assets/img/intro-carousel", fileName);
                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    ad.ImageUrl = relativePath.Replace("\\", "/"); // ذخیره مسیر در دیتابیس
                }

                _context.Add(ad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ad);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ad ad, IFormFile ImageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id != ad.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // پیدا کردن تبلیغ اصلی از دیتابیس
                    var existingAd = await _context.Ads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (existingAd == null) return NotFound();

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // نام منحصر به‌فرد فایل (برای جلوگیری از کش)
                        var fileName = $"{Path.GetFileNameWithoutExtension(ImageFile.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(ImageFile.FileName)}";
                        var relativePath = Path.Combine("assets/img/intro-carousel", fileName).Replace("\\", "/");
                        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                        // ذخیره عکس جدید
                        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
                        using (var stream = new FileStream(absolutePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // آپدیت آدرس عکس
                        ad.ImageUrl = relativePath;
                    }
                    else
                    {
                        // اگر کاربر عکس جدیدی انتخاب نکرده، عکس قبلی حفظ شود
                        ad.ImageUrl = existingAd.ImageUrl;
                    }

                    _context.Update(ad);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Ads.Any(e => e.Id == ad.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(ad);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var ad = await _context.Ads.FirstOrDefaultAsync(m => m.Id == id);
            if (ad == null) return NotFound();

            return View(ad);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var ad = await _context.Ads.FindAsync(id);
            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}