using System;
using System.Collections.Generic;
using System.Text;

namespace iSpeakMobile.Models
{
    public class Invoice
    {
        public Guid SaleInvoiceItems_Id { get; set; }
        public string No { get; set; }
        public string Package { get; set; }
        public string Price { get; set; }
        public string Due { get; set; }
        public string RemainingHours { get; set; }
        public string Status { get; set; }
    }
}
