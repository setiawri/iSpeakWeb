using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSpeakMobile.Models
{
    public class Session
    {
        public Guid SaleInvoiceItems_Id { get; set; }
        public string Date { get; set; }
        public string Lesson { get; set; }
        public decimal Hour { get; set; }
        public string Tutor { get; set; }
        public string Review { get; set; }
    }
}
