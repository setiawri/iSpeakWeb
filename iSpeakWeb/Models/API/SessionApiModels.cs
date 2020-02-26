using System;

namespace iSpeak.Models.API
{
    public class SessionApiModels
    {
        public Guid SaleInvoiceItems_Id { get; set; }
        public string Date { get; set; }
        public string Lesson { get; set; }
        public decimal Hour { get; set; }
        public string Tutor { get; set; }
        public string Student { get; set; }
        public string Review { get; set; }
    }
}