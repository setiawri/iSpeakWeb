using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class StockOnHandViewModels
    {
        public string Product { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Qty { get; set; }
        public string Unit { get; set; }
        [Display(Name = "For Sale")]
        public bool ForSale { get; set; }
    }
}