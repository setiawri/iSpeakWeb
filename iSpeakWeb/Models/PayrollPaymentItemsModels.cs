using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PayrollPaymentItems")]
    public class PayrollPaymentItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? PayrollPayments_Id { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Description { get; set; }
        public decimal Hour { get; set; }
        public decimal HourlyRate { get; set; }
        public int TutorTravelCost { get; set; }
        public decimal Amount { get; set; }
        public string UserAccounts_Id { get; set; }
    }
}