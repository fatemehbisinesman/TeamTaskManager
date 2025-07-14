using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TeamTaskManager.Data;
using TeamTaskManager.Models;

namespace TeamTaskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // دریافت لیست تبلیغات
            var ads = await _context.Ads.ToListAsync();

            // دریافت نظرات تأییدشده
            var approvedComments = await _context.Comments
                .Where(c => c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // ارسال هر دو به View
            ViewBag.ApprovedComments = approvedComments;
            return View(ads);
        }
    }
}
