using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using TeamTaskManager.Models;
using TeamTaskManager.Data;
using Microsoft.EntityFrameworkCore;
using System;

public class GroupProjectsController : Controller
{
    private readonly AppDbContext _context;

    public GroupProjectsController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // نقش کاربر جاری رو بگیر و در ویو ثبت کن
        ViewBag.CurrentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "کاربر";

        // دریافت پروژه همراه با اعضا و فایل‌ها
        var project = _context.Projects
            .Include(p => p.Members).ThenInclude(pm => pm.User)
            .Include(p => p.Files)
               .Include(p => p.Tasks)
            .FirstOrDefault(p => p.Id == id);

        if (project == null)
            return RedirectToAction("Index", "Orders"); // یا صفحه خطا دلخواه

        // ذخیره‌سازی در سشن (اختیاری)
        HttpContext.Session.SetInt32("CurrentProjectId", id);

        // محاسبه تعداد و مبلغ پرداختی‌ها
        var paidAmount = _context.Payments
            .Where(p => p.ProjectId == id && p.Status == "پرداخت شده")
            .Sum(p => (decimal?)p.Amount) ?? 0;

        ViewBag.PaidAmount = paidAmount;

        // محاسبه درصد پیشرفت
        var tasks = _context.Tasks.Where(t => t.ProjectId == id).ToList();
        int totalTasks = tasks.Count;
        int completedTasks = tasks.Count(t => t.IsCompleted); // فرض بر اینکه وجود دارد

        int progressPercent = 0;
        if (totalTasks > 0)
            progressPercent = (int)Math.Round((double)completedTasks / totalTasks * 100);

        ViewBag.ProgressPercent = progressPercent;

        return View(project);
    }



}
