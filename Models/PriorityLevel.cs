using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public enum PriorityLevel
    {
        [Display(Name = "پایین")]
        Low = 1,
        [Display(Name = "متوسط")]
        Medium = 3,
        [Display(Name = "بالا")]
        High = 5
    }
}
