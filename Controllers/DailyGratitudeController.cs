using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TeamTaskManager.Data;
using TeamTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System;

[Authorize]
public class DailyGratitudeController : Controller
{
    private readonly AppDbContext _context;

    public DailyGratitudeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string activeTab)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var gratitudes = await _context.DailyGratitudes
            .Where(g => g.UserId == userId)
            .ToListAsync();

        ViewBag.ActiveTab = activeTab ?? "شنبه";
        return View(gratitudes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DailyGratitude model)
    {
        // موقتاً اعتبارسنجی رو رد کن تا ببینیم مشکل از کجاست
        model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        model.Date = DateTime.Now;

        _context.DailyGratitudes.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.DailyGratitudes.FindAsync(id);
        if (item != null)
        {
            _context.DailyGratitudes.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}