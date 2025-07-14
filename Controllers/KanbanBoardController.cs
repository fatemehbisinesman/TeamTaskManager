using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Data;
using TeamTaskManager.Models;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Controllers
{
    public class KanbanBoardController : Controller
    {
        private readonly AppDbContext _context;

        public KanbanBoardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ProjectBoard(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId); // حذف شرط Type

            if (project == null)
                return NotFound();

            var groupedTasks = project.Tasks
                .Where(t => !string.IsNullOrEmpty(t.ColumnName))
                .GroupBy(t => t.ColumnName)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.OrderInColumn).ToList());

            ViewBag.GroupedTasks = groupedTasks;
            ViewBag.ProjectTitle = project.Title;
            ViewBag.ProjectId = project.Id;

            return View();
        }
        public async Task<IActionResult> Report(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var totalTasks = project.Tasks.Count;
            var toDoCount = project.Tasks.Count(t => t.ColumnName == "To Do");
            var doingCount = project.Tasks.Count(t => t.ColumnName == "Doing");
            var doneCount = project.Tasks.Count(t => t.ColumnName == "Done");

            ViewBag.ProjectTitle = project.Title;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.ToDoCount = toDoCount;
            ViewBag.DoingCount = doingCount;
            ViewBag.DoneCount = doneCount;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            int projectId = task.ProjectId;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProjectBoard", new { projectId = projectId });
        }
        [HttpGet]
        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            return View(task);
        }
        [HttpPost]
        public async Task<IActionResult> EditTask(TaskItem updatedTask)
        {
            var task = await _context.TaskItems.FindAsync(updatedTask.Id);
            if (task == null)
                return NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.ColumnName = updatedTask.ColumnName;

            await _context.SaveChangesAsync();

            return RedirectToAction("ProjectBoard", new { projectId = task.ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(string Title, string Description, string ColumnName, int ProjectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == ProjectId);

            if (project == null) return NotFound();

            int order = project.Tasks.Count(t => t.ColumnName == ColumnName);

            var task = new TaskItem
            {
                Title = Title,
                Description = Description,
                ColumnName = ColumnName,
                OrderInColumn = order,
                ProjectId = ProjectId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProjectBoard", new { projectId = ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskPosition([FromBody] TaskPositionUpdateModel model)
        {
            var task = await _context.TaskItems.FindAsync(model.TaskId);
            if (task == null) return NotFound();

            task.ColumnName = model.NewColumnName;
            task.OrderInColumn = model.NewOrder;

            await _context.SaveChangesAsync();
            return Ok();
        }
        // GET: تنظیمات پروژه
        public async Task<IActionResult> ProjectSettings(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: ویرایش تنظیمات
        [HttpPost]
        public async Task<IActionResult> ProjectSettings(Project project)
        {
            var existing = await _context.Projects.FindAsync(project.Id);
            if (existing == null)
                return NotFound();

            existing.Title = project.Title;
            existing.IsActive = project.IsActive;
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ تنظیمات ذخیره شد.";
            return RedirectToAction("ProjectBoard", new { projectId = project.Id });
        }

        // POST: حذف پروژه
        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            // حذف وابستگی‌ها
            _context.TaskItems.RemoveRange(project.Tasks);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            TempData["Message"] = "❌ پروژه با موفقیت حذف شد.";
            return RedirectToAction("Index", "Orders");
        }

        public class TaskPositionUpdateModel
        {
            public int TaskId { get; set; }
            public string NewColumnName { get; set; }
            public int NewOrder { get; set; }
        }

    }
}
