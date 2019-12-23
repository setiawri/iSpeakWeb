using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class SaleReturnViewModels
    {
        public Guid Id { get; set; }
        public string No { get; set; }
        public string Timestamp { get; set; }
        public string Notes { get; set; }
        public bool Approved { get; set; }
    }

    public class SaleReturnDetails
    {
        public string description { get; set; }
        public int qty_inv { get; set; }
        public int qty_returned { get; set; }
        public int qty_return { get; set; }
        public Guid inv_item_id { get; set; }
    }
}