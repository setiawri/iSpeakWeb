using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class TutorStudentSchedulesViewModels
    {
        public Guid Id { get; set; }
        public string Tutor { get; set; }
        public string Student { get; set; }
        [Display(Name = "Day of Week")]
        public DayOfWeekEnum DayOfWeek { get; set; }
        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}")]
        public DateTime EndTime { get; set; }
        [Display(Name = "Lesson")]
        public string Invoice { get; set; }
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}