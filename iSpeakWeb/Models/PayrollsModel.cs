using System;
using System.ComponentModel.DataAnnotations;

namespace iSpeak.Models
{
    public class PayrollsModel
    {
        public string Tutor_UserAccounts_Id { get; set; }

        [Display(Name = "Name")]
        public string Tutor_UserAccounts_FullName { get; set; }

        [Display(Name = "Hours")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalHours { get; set; }

        [Display(Name = "Payable")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PayableAmount { get; set; }

        [Display(Name = "Due")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal DueAmount { get; set; }
    }
}