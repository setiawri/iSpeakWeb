using System;
using System.Collections.Generic;
using System.Text;

namespace iSpeakMobile.Models
{
    public class Schedule
    {
        public Guid Schedule_Id { get; set; }
        public string TimeSchedule { get; set; }
        public string Lesson { get; set; }
        public string Tutor { get; set; }
        public string Notes { get; set; }
        public string Role { get; set; }
    }
}
