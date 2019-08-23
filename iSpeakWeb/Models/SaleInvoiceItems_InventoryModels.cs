using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleInvoiceItems_Inventory")]
    public class SaleInvoiceItems_InventoryModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SaleInvoiceItems_Id { get; set; }
        public Guid Inventory_Id { get; set; }
        public int Qty { get; set; }
    }
}