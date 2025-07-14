using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public class ProjectFile
    {
        public int Id { get; set; }


        [Required]
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;
    }
}
