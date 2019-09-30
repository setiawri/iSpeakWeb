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
        public int Due { get; set; }
        public bool Cancelled { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SaleInvoicesIndexModels
    {
        public Guid Id { get; set; }
        public string Branches { get; set; }
        public string No { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public string Customer { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Amount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Due { get; set; }
        public bool Cancelled { get; set; }
        [Display(Name = "Approved")]
        public bool IsChecked { get; set; }
        public string Notes { get; set; }
    }

    public class SaleInvoiceItemDetails
    {
        public string desc { get; set; }
        public int qty { get; set; }
        public int price { get; set; }
        public int travel { get; set; }
        public int tutor { get; set; }
        public decimal disc { get; set; }
        public decimal voucher { get; set; }
        public int subtotal { get; set; }
        public decimal hours { get; set; }
        public string voucher_id { get; set; }
        public string note { get; set; }
        public Guid? lesson_id { get; set; }
        public Guid? inventory_id { get; set; }
        public Guid? service_id { get; set; }

    }
}