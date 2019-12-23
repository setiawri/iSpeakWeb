using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Models
{
    [Table("LessonSessions")]
    public class LessonSessionsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? Branches_Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid SaleInvoiceItems_Id { get; set; }
        public decimal SessionHours { get; set; }
        public string Review { get; set; }
        [Display(Name = "Internal Notes")]
        public string InternalNotes { get; set; }
        public bool Deleted { get; set; }
        [Required]
        [Display(Name = "Tutor")]
        public string Tutor_UserAccounts_Id { get; set; }
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRates_Rate { get; set; }
        [Display(Name = "Travel Cost")]
        public int TravelCost { get; set; }
        [Display(Name = "Tutor Travel Cost")]
        public int TutorTravelCost { get; set; }
        public decimal Adjustment { get; set; }
        public Guid? PayrollPaymentItems_Id { get; set; }
        public string Notes_Cancel { get; set; }
    }

    public class LessonSessionsDetails
    {
        public string student { get; set; }
        public string package { get; set; }
        public string review { get; set; }
        public string internal_notes { get; set; }
        public Guid sale_invoice_item_id { get; set; }
    }
}