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
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalHours { get; set; }
        [Display(Name = "Total Payable")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalPayable { get; set; }
        public string Details { get; set; }
    }
}