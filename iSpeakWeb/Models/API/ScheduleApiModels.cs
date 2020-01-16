using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models.API
{
    public class ScheduleApiModels
    {
        public Guid Schedule_Id { get; set; }
        public string TimeSchedule { get; set; }
        public string Lesson { get; set; }
        public string Tutor { get; set; }
        public string Notes { get; set; }
        public string Role { get; set; }
    }
}