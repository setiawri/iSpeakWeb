using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class TutorSchedulesSearchViewModels
    {
        [Display(Name = "Language")]
        public Guid Languages_Id { get; set; }
        [Display(Name = "Tutor")]
        public string Tutor_Id { get; set; }
        [Display(Name = "Day of Week")]
        public DayOfWeekEnum DayOfWeek { get; set; }
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
    }
}