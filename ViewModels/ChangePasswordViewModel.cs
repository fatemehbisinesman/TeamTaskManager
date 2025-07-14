using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.ViewModels
{

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "رمز عبور باید حداقل ۸ کاراکتر باشد")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
        public string ConfirmPassword { get; set; }
    }

}


