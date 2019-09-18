using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PayrollPayments")]
    public class PayrollPaymentsModels
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string No { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        [Required]
        public string UserAccounts_Id { get; set; }
        public bool IsChecked { get; set; }
        public bool Cancelled { get; set; }
    }
}