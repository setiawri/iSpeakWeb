﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Payments")]
    public class PaymentsModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string No { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        [Display(Name = "Cash")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CashAmount { get; set; }
        [Display(Name = "Debit")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int DebitAmount { get; set; }
        public string DebitBank { get; set; }
        public string DebitOwnerName { get; set; }
        public string DebitNumber { get; set; }
        public string DebitRefNo { get; set; }
        public Guid? Consignments_Id { get; set; }
        [Display(Name = "Consignment")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ConsignmentAmount { get; set; }
        public string Notes { get; set; }
        public bool Cancelled { get; set; }
        public bool Confirmed { get; set; }
        public bool IsTransfer { get; set; }
        public string Notes_Cancel { get; set; }
    }

    public class PaymentsIndexModels
    {
        public Guid Id { get; set; }
        public string No { get; set; }
        [Display(Name = "Date")]
        //[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public string Timestamp { get; set; }
        [Display(Name = "Cash")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CashAmount { get; set; }
        [Display(Name = "Debit")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int DebitAmount { get; set; }
        [Display(Name = "Consignment")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ConsignmentAmount { get; set; }
        public string Branch { get; set; }
        public bool Cancelled { get; set; }
        [Display(Name = "Approved")]
        public bool Confirmed { get; set; }
        public string Notes_Cancel { get; set; }
        public bool HasSession { get; set; }
    }

    public class PaymentIndexSQL
    {
        public Guid Id { get; set; }
        public string Branch { get; set; }
        public DateTime Date { get; set; }
        public string No { get; set; }
        public int Cash { get; set; }
        public int Debit { get; set; }
        public int Consignment { get; set; }
        public bool Cancelled { get; set; }
        public bool Confirmed { get; set; }
        public string Notes_Cancel { get; set; }
        public bool Has_Session { get; set; }
    }
}