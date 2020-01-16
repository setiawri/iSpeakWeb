using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models.API
{
    public class TutorScheduleApiModels
    {
        public string Username { get; set; }
        public string Schedule_Id { get; set; }
        public string Day { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Notes { get; set; }
    }
}