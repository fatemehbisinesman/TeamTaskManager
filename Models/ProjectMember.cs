using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "نقش کاربر الزامی است")]
        [Display(Name = "نقش")]
        public string Role { get; set; } = "Member"; // مقدار پیش‌فرض: Member

        [Display(Name = "تاریخ عضویت")]
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // روابط
        public virtual Project? Project { get; set; }
        public virtual User? User { get; set; }
    }
}
