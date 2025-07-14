using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان پروژه الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان نباید بیشتر از 100 کاراکتر باشد")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "تاریخ ایجاد")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "وضعیت پرداخت")]
        public bool IsPaid { get; set; } = false;

        [Required(ErrorMessage = "لطفاً قیمت را وارد کنید")]
        [Range(0, int.MaxValue, ErrorMessage = "قیمت باید صفر یا بیشتر باشد")]
        [Display(Name = "قیمت")]
        public int Price { get; set; }


        [Display(Name = "وضعیت پروژه")]
        public string Status { get; set; } = "Pending"; // یا مثلاً "فعال"، "در انتظار تایید"...

        [Display(Name = "فعال است؟")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "ایجادشده توسط (UserId)")]
        public int CreatedBy { get; set; }

        [Display(Name = "آدرس تصویر کاور")]
        public string CoverImagePath { get; set; } = string.Empty;

        [Required]
        [Display(Name = "نوع پروژه")]
        public string Type { get; set; } = "Group";  // مقادیر: Group, Kanban, Daily و ...



        // روابط (Navigation Properties)
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        [ValidateNever]
        public virtual ICollection<ProjectFile> Files { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
