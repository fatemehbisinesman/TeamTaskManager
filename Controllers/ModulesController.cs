using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using TeamTaskManager.Models;
using TeamTaskManager.ViewModels;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class ModulesController : Controller
    {
        private readonly AppDbContext _context;

        public ModulesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult AvailableModules()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var ownedModules = _context.UserModules
                .Where(x => x.UserId == userId.Value)
                .Select(x => x.ModuleName)
                .ToList();

            var staticModules = new List<ModuleViewModel>
            {
                new ModuleViewModel { Name = "Planning", NameFa = "برنامه‌ریزی روزانه", Price = 0, IsOwned = ownedModules.Contains("Planning"), IsProject = false },
                new ModuleViewModel { Name = "Report", NameFa = "گزارش‌ها", Price = 20000, IsOwned = ownedModules.Contains("Report"), IsProject = false },
            };

            var ownedProjectIds = _context.UserModules
                .Where(x => x.UserId == userId.Value && x.IsProject && x.IsPaid)
                .Select(x => x.ProjectId.Value)
                .ToList();

            var projectModules = _context.Projects
                .Where(p => p.IsActive && !ownedProjectIds.Contains(p.Id))
                .Select(p => new ModuleViewModel
                {
                    Name = $"Project_{p.Id}",
                    NameFa = p.Title,
                    Price = p.Price,
                    IsOwned = false,
                    IsProject = true,
                    ProjectId = p.Id
                }).ToList();

            var allModules = staticModules.Concat(projectModules).ToList();

            return View(allModules);
        }

        public IActionResult Buy(string module)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var exists = _context.UserModules
                .Any(x => x.UserId == userId && x.ModuleName == module);

            if (!exists)
            {
                _context.UserModules.Add(new UserModule
                {
                    UserId = userId.Value,
                    ModuleName = module,
                    IsProject = false,
                    IsPaid = true
                });
                _context.SaveChanges();
            }

            return RedirectToAction("AvailableModules");
        }

        // PayProject و Verify بعد از راه‌اندازی Parbad اضافه می‌شود
    }
}
