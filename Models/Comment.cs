using System;
using System.ComponentModel.DataAnnotations;

namespace TeamTaskManager.Models
{
    public class Comment
    {

        public int Id { get; set; }

       
            [Required]
    public int UserId { get; set; }

        [Required(ErrorMessage = "متن نظر نمی‌تواند خالی باشد.")]
        [MaxLength(1000, ErrorMessage = "متن نظر نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        // Navigation Property
        public virtual User? User { get; set; }
    }
}

