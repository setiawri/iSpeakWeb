using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleReturnItems")]
    public class SaleReturnItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SaleReturns_Id { get; set; }
        public Guid SaleInvoiceItems_Id { get; set; }
        public int Qty { get; set; }
    }
}