using System;
using System.Collections.Generic;
using System.Text;

namespace iSpeakMobile.Models
{
    public class TutorSchedule
    {
        public Guid Id { get; set; }
        public string Tutor_UserAccounts_Id { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}
