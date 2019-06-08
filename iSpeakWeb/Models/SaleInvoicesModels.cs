using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleInvoices")]
    public class SaleInvoicesModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        [Required]
        public string No { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Customer_UserAccounts_Id { get; set; }
        public string Notes { get; set; }
        public int Amount { get; set; }
        public bool Cancelled { get; set; }
    }

    public class SaleInvoicesIndexModels
    {
        public Guid Id { get; set; }
        public string Branches { get; set; }
        public string No { get; set; }
        public DateTime Timestamp { get; set; }
        public string Customer { get; set; }
        public int Amount { get; set; }
        public bool Cancelled { get; set; }
    }
}