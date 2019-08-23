using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Inventory")]
    public class InventoryModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        [Display(Name = "Product")]
        public Guid Products_Id { get; set; }
        [Display(Name = "Receive Date")]
        public DateTime ReceiveDate { get; set; }
        [Display(Name = "Buy Qty")]
        public int BuyQty { get; set; }
        public int AvailableQty { get; set; }
        [Display(Name = "Supplier")]
        public Guid Suppliers_Id { get; set; }
        [Display(Name = "Buy Price")]
        public int BuyPrice { get; set; }
        public string Notes { get; set; }
    }

    public class InventoryViewModels
    {
        public Guid Id { get; set; }
        public string Branch { get; set; }
        public string Product { get; set; }
        public string Unit { get; set; }
        [Display(Name = "Receive Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime ReceiveDate { get; set; }
        [Display(Name = "Buy Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int BuyQty { get; set; }
        [Display(Name = "Avail. Qty")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AvailableQty { get; set; }
        public string Supplier { get; set; }
        [Display(Name = "Buy Price")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int BuyPrice { get; set; }
    }
}