using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class LessonSessionsViewModels
    {
        public Guid Id { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public string Lesson { get; set; }
        public string Tutor { get; set; }
        [Display(Name = "Hour")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SessionHours { get; set; }
        [Display(Name = "Hourly Rate")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal HourlyRates_Rate { get; set; }
        [Display(Name = "Travel Cost")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TravelCost { get; set; }
        [Display(Name = "Tutor Cost")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TutorTravelCost { get; set; }
        public bool Deleted { get; set; }
    }
}