using System;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "نام کاربر الزامی است")]
        [Display(Name = "نام کاربر")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "پرداخت بابت الزامی است")]
        [Display(Name = "پرداخت بابت")]
        public string PaymentFor { get; set; } = string.Empty;

        [Required(ErrorMessage = "مبلغ پرداخت الزامی است")]
        [Range(0, int.MaxValue, ErrorMessage = "مقدار مبلغ معتبر نیست")]
        [Display(Name = "مبلغ پرداخت")]
        public int Amount { get; set; }

        [Display(Name = "تاریخ پرداخت")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "وضعیت")]
        public string Status { get; set; } = "در انتظار پرداخت";

        [Display(Name = "کد پیگیری")]
        public string? RefId { get; set; }

        [Display(Name = "شناسه پروژه (در صورت وجود)")]
        public int? ProjectId { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ Navigation Properties
        public virtual Project? Project { get; set; }
        public virtual User? User { get; set; }
    }
}
