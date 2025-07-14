using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رمز عبور جدید الزامی است.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد.")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "رمزها با هم مطابقت ندارند.")]
        public string ConfirmPassword { get; set; }
    }
}