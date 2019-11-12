using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class StudentPackageViewModels
    {
        public Guid SaleInvoices_Id { get; set; }
        public string No { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public List<SaleInvoiceItemsModels> SaleInvoiceItems { get; set; }
        public string StudentName { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Amount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Due { get; set; }
        public bool Cancelled { get; set; }
    }
}