using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public string Link { get; set; } = default!;
    }
}
