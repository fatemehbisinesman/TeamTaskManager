using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeamTaskManager.Models
{
    public class DailyPlanItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان برنامه الزامی است")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "تاریخ برنامه‌ریزی الزامی است")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public bool IsCompleted { get; set; }

        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public string? Tag { get; set; }
        public DateTime? ReminderTime { get; set; }


        public string UserId { get; set; }

        [BindNever] // جلوگیری از اعتبارسنجی
        public User User { get; set; }
    }
}