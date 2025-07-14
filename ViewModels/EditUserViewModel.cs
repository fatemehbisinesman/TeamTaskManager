using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "نقش کاربر الزامی است")]
        public string Role { get; set; } = "User";

        public string? ProfileImage { get; set; }
    }
}
