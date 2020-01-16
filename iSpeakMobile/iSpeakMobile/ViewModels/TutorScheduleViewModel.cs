using System;
using System.Collections.Generic;
using System.Text;

namespace iSpeakMobile.ViewModels
{
    public class TutorScheduleViewModel
    {
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}
