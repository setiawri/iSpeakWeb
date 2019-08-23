using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class ReceiptViewModels
    {
        public BranchesModels Branch { get; set; }
        public PaymentsModels Payment { get; set; }
        public List<SaleInvoiceItemsDetails> listSaleInvoiceItems { get; set; }
        public List<PaymentItemsDetails> listPaymentItems { get; set; }
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public decimal TotalCash { get; set; }
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public decimal TotalDebit { get; set; }
        public string ConsignmentName { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }
    }
}