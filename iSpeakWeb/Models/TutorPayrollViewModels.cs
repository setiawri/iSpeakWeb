using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class TutorPayrollViewModels
    {
        public string TutorId { get; set; }
        [Display(Name = "Tutor Name")]
        public string Name { get; set; }
        [Display(Name = "Total Hours")]
        public string TotalHours { get; set; }
        [Display(Name = "Total Payable")]
        public string TotalPayable { get; set; }
        public string Details { get; set; }
    }

    public class TutorPayrollDetailsViewModels
    {
        public Guid Id { get; set; }
        public string Timestamp { get; set; }
        public string Description { get; set; }
        public string SessionHours { get; set; }
        public string HourlyRate { get; set; }
        public string Amount { get; set; }
        public string Paid { get; set; }
    }
}