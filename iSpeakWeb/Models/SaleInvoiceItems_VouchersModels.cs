using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleInvoiceItems_Vouchers")]
    public class SaleInvoiceItems_VouchersModels
    {
        [Key]
        public Guid Id { get; set; }
        public string Voucher_Ids { get; set; }
        public decimal Amount { get; set; }
    }
}