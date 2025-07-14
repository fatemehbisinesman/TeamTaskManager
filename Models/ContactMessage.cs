using System;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "لطفاً نام را وارد کنید")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "لطفاً ایمیل را وارد کنید")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "موضوع الزامی است")]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "متن پیام الزامی است")]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
