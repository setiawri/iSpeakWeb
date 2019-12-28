using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("TutorSchedules")]
    public class TutorSchedulesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [Display(Name = "Tutor")]
        public string Tutor_UserAccounts_Id { get; set; }
        [Display(Name = "Day of Week")]
        public DayOfWeekEnum DayOfWeek { get; set; }
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}