using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models.API
{
    public class InvoiceApiModels
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