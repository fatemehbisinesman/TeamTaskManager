using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTaskManager.ViewModels
{
    public class ModuleViewModel
    {
        public string Name { get; set; }
        public string NameFa { get; set; }
        public int Price { get; set; }
        public bool IsOwned { get; set; }
        public int? ProjectId { get; set; } // برای تشخیص پروژه‌های ادمین
        public bool IsProject { get; set; } // اگر true باشد یعنی این ماژول در واقع یک پروژه است
    }
}
