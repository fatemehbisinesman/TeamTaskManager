using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeamTaskManager.Data;
using TeamTaskManager.Models;

public class ProjectMembersController : Controller
{
    private readonly AppDbContext _context;

    public ProjectMembersController(AppDbContext context)
    {
        _context = context;
    }

    // نمایش لیست اعضای پروژه
    public IActionResult Index(int projectId)
    {
        var members = _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .ToList();

        ViewBag.ProjectId = projectId;
        return View(members);
    }

    // صفحه افزودن عضو
    public IActionResult AddMember(int projectId)
    {
        // فهرست کاربران ثبت‌نام‌شده
        var users = _context.Users.ToList();
        ViewBag.Users = users;
        ViewBag.ProjectId = projectId;
        return View();
    }
    [HttpPost]
    public IActionResult Add(int projectId, int userId, string role)
    {
        // نقش مدیر رو وارسی کنید
        if (role == "مدیر")
        {
            // اگر در پروژه عضو دیگری نقش مدیر دارد، مجاز نیست دو مدیر باشند، یا آن نقش را تغییر بده
        }

        // ثبت جدید
        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.Now
        };
        _context.ProjectMembers.Add(member);
        _context.SaveChanges();

        return RedirectToAction("Members", new { projectId });
    }

    public IActionResult Create(Project model)
    {
        if (ModelState.IsValid)
        {
            _context.Projects.Add(model);
            _context.SaveChanges();

            // نقش کاربر جاری رو بگیر
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            // نقش مدیر برای کاربر جاری ثبت کن
            var projectMember = new ProjectMember
            {
                ProjectId = model.Id,
                UserId = userId,
                Role = "مدیر",
                JoinedAt = DateTime.Now
            };

            _context.ProjectMembers.Add(projectMember);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", "GroupProjects", new { id = model.Id });
        }

        return View(model);
    }
    // ثبت عضو جدید
    [HttpPost]
    public IActionResult AddMember(int projectId, int userId, string role)
    {
        if (!_context.Users.Any(u => u.Id == userId))
            return BadRequest("کاربر معتبر نیست.");

        var exists = _context.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.UserId == userId);
        if (exists)
            return BadRequest("کاربر قبلاً عضو شده است.");

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.Now
        };
        _context.ProjectMembers.Add(member);
        _context.SaveChanges();

        return RedirectToAction("Index", "ProjectMembers",new { projectId });
    }
    public IActionResult Add(int projectId)
    {
        ViewBag.Users = _context.Users.ToList(); // لیست کاربران ثبت‌نام شده
        ViewBag.ProjectId = projectId;               // شناسه پروژه
        return View();
    }

    public IActionResult Members(int projectId)
    {
        var members = _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .ToList();
        ViewBag.ProjectId = projectId;
        return View(members);
    }
    // حذف عضو

    public IActionResult Delete(int id, int projectId)
    {
        var member = _context.ProjectMembers.Find(id);
        if (member != null)
        {
            _context.ProjectMembers.Remove(member);
            _context.SaveChanges();
        }
        return RedirectToAction("Index", new { projectId });
    }
}