using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamTaskManager.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [Required(ErrorMessage = "مبلغ الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ معتبر وارد کنید")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "وضعیت پرداخت الزامی است")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // مثلاً "پرداخت شده"، "در انتظار"، "ناموفق"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
