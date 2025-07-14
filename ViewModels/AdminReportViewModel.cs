using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.ViewModels
{
    public class AdminReportViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalNormalUsers { get; set; }
        public int TotalProjects { get; set; }
        public int TotalPayments { get; set; }
        public int PaidProjects { get; internal set; }
        public int AllProjects { get; internal set; }
    }
}
