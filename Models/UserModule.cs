using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public class UserModule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Display(Name = "نام ماژول")]
        public string? ModuleName { get; set; }  // اگر پروژه باشد، ممکن است خالی بماند

        [Display(Name = "وضعیت پرداخت")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "تاریخ خرید")]
        public DateTime PurchasedAt { get; set; } = DateTime.Now;

        [Display(Name = "پروژه مرتبط")]
        public int? ProjectId { get; set; }

        [Display(Name = "آیا ماژول پروژه است؟")]
        public bool IsProject { get; set; } = false;

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }


    }

}
