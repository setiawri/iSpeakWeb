using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class ReceiptPayrollViewModels
    {
        public DateTime PayrollDate { get; set; }
        public string TutorName { get; set; }
        public List<PayrollByStudent> ListPayroll { get; set; }
        public List<PayrollManualReceipt> ListPayrollManual { get; set; }
    }

    public class PayrollByStudent
    {
        public string Student_Id { get; set; }
        public string StudentName { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalHours { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalRate { get; set; }
    }

    public class PayrollManualReceipt
    {
        public string Description { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
    }
}