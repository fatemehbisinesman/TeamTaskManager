using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamTaskManager.Data;
using TeamTaskManager.Models;

public class FilesController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;

    private readonly string[] permittedExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".txt", ".pdf", ".docx", ".xlsx", ".jpg", ".png", ".jpeg", ".gif", ".mp4", ".mp3", ".csv" }; // می‌توانی اضافه/کاهش دهی

    private const long maxFileSize = 5 * 1024 * 1024; // 5 مگابایت

    public FilesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: /Files/Upload?projectId=5
    public IActionResult Upload(int projectId)
    {
        ViewBag.ProjectId = projectId;
        return View();
    }
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var file = _context.ProjectFiles.Find(id);
        if (file != null)
        {
            _context.ProjectFiles.Remove(file);
            _context.SaveChanges();
        }
        return RedirectToAction("Index", "Files", new { projectId = file.ProjectId }); // به نمای Index بازگردید
    }
    // POST: /Files/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int projectId, IFormFile uploadFile)
    {
        if (uploadFile == null || uploadFile.Length == 0)
        {
            ModelState.AddModelError("", "لطفا یک فایل انتخاب کنید.");
            ViewBag.ProjectId = projectId;
            return View();
        }

        if (uploadFile.Length > maxFileSize)
        {
            ModelState.AddModelError("", "حجم فایل نباید بیش از 5 مگابایت باشد.");
            ViewBag.ProjectId = projectId;
            return View();
        }

        var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || Array.IndexOf(permittedExtensions, ext) < 0)
        {
            ModelState.AddModelError("", "نوع فایل انتخاب شده پشتیبانی نمی‌شود.");
            ViewBag.ProjectId = projectId;
            return View();
        }

        // مسیر فولدر uploads در روت پروژه
        string uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // نام فایل جدید با GUID تا از تداخل جلوگیری شود
        var uniqueFileName = Guid.NewGuid().ToString() + ext;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await uploadFile.CopyToAsync(stream);
        }

        // ذخیره اطلاعات فایل در دیتابیس (در جدول Files که باید داشته باشید)
        var fileRecord = new ProjectFile
        {
            ProjectId = projectId,
            FileName = uploadFile.FileName, // نام واقعی فایل برای نمایش
            FilePath = "/uploads/" + uniqueFileName,
            UploadedAt = DateTime.Now
        };

        _context.ProjectFiles.Add(fileRecord);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { projectId = projectId });
    }

    // GET: /Files/Index?projectId=5
    public IActionResult Index(int projectId) // متد برای خواندن فایل‌ها بر اساس projectId
    {
        ViewBag.ProjectId = projectId; // ارسال projectId به ویو
        var files = _context.ProjectFiles.Where(f => f.ProjectId == projectId).ToList(); // لیست فایل‌ها
        return View(files); // ارسال لیست فایل‌ها به ویو
    }
    public IActionResult Details(int id)
    {
        var file = _context.ProjectFiles
            .Include(f => f.Project) // اگر به اطلاعات پروژه نیاز دارید
            .FirstOrDefault(f => f.Id == id);

        if (file == null)
            return NotFound();

        return View(file);
    }
}