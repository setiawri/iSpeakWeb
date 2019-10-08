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
        public Guid? SaleInvoiceItems_Vouchers_Id { get; set; }
        public string Notes { get; set; }
        public Guid? Products_Id { get; set; }
        public Guid? Services_Id { get; set; }
        public Guid? LessonPackages_Id { get; set; }
        public decimal? SessionHours { get; set; }
        public decimal? SessionHours_Remaining { get; set; }
        public int TravelCost { get; set; }
        public int TutorTravelCost { get; set; }
    }

    public class SaleInvoiceItemsDetails
    {
        public string Invoice { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Price { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Travel { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Tutor { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Voucher { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Discount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
        public string Customer { get; set; }
    }
}