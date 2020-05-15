using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class PayrollPaymentsViewModels
    {
        public Guid Id { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public string No { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public string Tutor { get; set; }
        public bool Cancelled { get; set; }
        [Display(Name = "Approved")]
        public bool IsChecked { get; set; }
        public string Tutor_Id { get; set; }
        public string Notes_Cancel { get; set; }
    }
}