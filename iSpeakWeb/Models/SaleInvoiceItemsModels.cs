using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleInvoiceItems")]
    public class SaleInvoiceItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public byte RowNo { get; set; }
        public Guid SaleInvoices_Id { get; set; }
        [Required]
        public string Description { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public Guid? Vouchers_Id { get; set; }
        public string Notes { get; set; }
        public Guid? Products_Id { get; set; }
        public Guid? Services_Id { get; set; }
        public Guid? LessonPackages_Id { get; set; }
        public decimal? SessionHours { get; set; }
        public int TravelCost { get; set; }
        public int TutorTravelCost { get; set; }
    }
}