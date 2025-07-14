using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }  // کلید اصلی

        [Required(ErrorMessage = "نام ماژول الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام فارسی ماژول الزامی است")]
        [MaxLength(100)]
        public string NameFa { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "مقدار قیمت معتبر نیست")]
        public int Price { get; set; }

        public bool IsProject { get; set; } = false;

        public int? ProjectId { get; set; }
    }

}
