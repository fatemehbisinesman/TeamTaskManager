using Microsoft.AspNetCore.Mvc;
using TeamTaskManager.Models;
using System;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateComment(string Text, int UserId)
        {
            var comment = new Comment
            {
                Text = Text,
                UserId = UserId,
                CreatedAt = DateTime.Now,
                IsApproved = false
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            TempData["CommentSuccess"] = "✅ نظر شما با موفقیت ثبت شد و پس از تأیید توسط مدیر در سایت نمایش داده خواهد شد.";

            return RedirectToAction("Index", "Home");
        }

    }
}
