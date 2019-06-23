using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PaymentItems")]
    public class PaymentItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Payments_Id { get; set; }
        public Guid ReferenceId { get; set; }
        public int Amount { get; set; }
        public int DueBefore { get; set; }
        public int DueAfter { get; set; }
    }

    public class PaymentItemsDetails
    {
        public string Invoice { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Amount { get; set; }
        [Display(Name = "Due")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int DueBefore { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Payment { get; set; }
        [Display(Name = "Now")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int DueAfter { get; set; }
    }
}