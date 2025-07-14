using System;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class DailyGratitude
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "متن شکرگزاری الزامی است")]
        [StringLength(500, ErrorMessage = "حداکثر ۵۰۰ کاراکتر")]
        public string Text { get; set; }

        public DateTime Date { get; set; }
    }
}