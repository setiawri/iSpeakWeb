using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("TutorStudentSchedules")]
    public class TutorStudentSchedulesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [Display(Name = "Tutor")]
        public string Tutor_UserAccounts_Id { get; set; }
        [Required]
        [Display(Name = "Student")]
        public string Student_UserAccounts_Id { get; set; }
        public DayOfWeekEnum DayOfWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        [Display(Name = "Lesson")]
        public Guid InvoiceItems_Id { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}