using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamTaskManager.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان وظیفه الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان نباید بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "وضعیت")]
        public TaskStatus Status { get; set; } = TaskStatus.Doing;

        [Display(Name = "موعد انجام")]
        public DateTime? Deadline { get; set; }

        [Display(Name = "اولویت")]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        [Display(Name = "تاریخ ایجاد")]
        [Required(ErrorMessage = "تاریخ ایجاد الزامی است")]
        public DateTime CreatedAt { get; set; }

        // Foreign Key
        [Required]
        [Display(Name = "پروژه مرتبط")]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        [Display(Name = "تخصیص‌یافته به")]
        public int? AssignedUserId { get; set; }

        // Optional: رابطه با User
        // public virtual User? AssignedUser { get; set; }

        [Display(Name = "تکمیل شده")]
        public bool IsCompleted { get; set; } = false;
        [Display(Name = "ستون کانبان")]
        [StringLength(50)]
        public string? ColumnName { get; set; }  // مثال: "ToDo", "InProgress", "Done"

        [Display(Name = "ترتیب در ستون")]
        public int? OrderInColumn { get; set; }  // برای Drag & Drop


      


        [Display(Name = "توضیحات")]
        [StringLength(1000)]
        public string? Description { get; set; }

    }

    // تعریف Enum وضعیت وظیفه
    public enum TaskStatus
    {
        [Display(Name = "در حال انجام")]
        Doing,
        [Display(Name = "تکمیل شده")]
        Completed,
        [Display(Name = "لغو شده")]
        Canceled
    }
   
}