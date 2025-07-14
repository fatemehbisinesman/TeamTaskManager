using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        [StringLength(100, ErrorMessage = "نام نباید بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "رمز باید شامل حداقل یک حرف بزرگ، یک حرف کوچک، یک عدد و یک نماد خاص باشد")]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تصویر پروفایل")]
        public string? ProfileImage { get; set; } // Optional

        [Display(Name = "نقش کاربر")]
        public string Role { get; set; } = "User";

        [Display(Name = "تاریخ ثبت‌نام")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

    }
}