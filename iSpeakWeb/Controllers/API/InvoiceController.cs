using iSpeak.Models;
using iSpeak.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace iSpeak.Controllers.API
{
    public class InvoiceController : ApiController
    {
        private iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/invoice")]
        public HttpResponseMessage Invoices(CommonRequestModels model)
        {
            var invoices = (from si in db.SaleInvoices
                            join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                            join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            where si.Cancelled == false && sii.LessonPackages_Id != null && u.UserName == model.Username
                            select new { si, sii, lp, u }).ToList();
            List<InvoiceApiModels> list = new List<InvoiceApiModels>();
            if (invoices.Count > 0)
            {
                foreach (var invoice in invoices.OrderByDescending(x => x.si.Timestamp))
                {
                    list.Add(new InvoiceApiModels
                    {
                        No = "Invoice No. " + invoice.si.No,
                        Package = invoice.lp.Name,
                        Price = string.Format("{0} {1:N0}", "Rp", invoice.sii.Price),
                        Due = string.Format("{0} {1:N0}", "Rp", invoice.si.Due),
                        TotalHours = invoice.sii.SessionHours.Value,
                        RemainingHours = invoice.sii.SessionHours_Remaining.Value,
                        Status = invoice.si.Due > 0 ? "Waiting Payment" : "Completed"
                    });
                }
            }

            if (list == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/payment")]
        public HttpResponseMessage CheckPayments(CommonRequestModels model)
        {
            var payments = (from p in db.Payments
                            join pi in db.PaymentItems on p.Id equals pi.Payments_Id
                            join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                            join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            join b in db.Branches on si.Branches_Id equals b.Id
                            where u.UserName == model.Username
                            select new { p, pi, si, sii, u, b }).ToList();
            List<PaymentApiModels> list = new List<PaymentApiModels>();
            if (payments.Count > 0)
            {
                foreach (var payment in payments.OrderByDescending(x => x.p.Timestamp))
                {
                    list.Add(new PaymentApiModels
                    {
                        No = "Payment No. " + payment.p.No,
                        Date = string.Format("{0:yyyy/MM/dd}", TimeZoneInfo.ConvertTimeFromUtc(payment.p.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                        Amount = string.Format("{0} {1:N0}", "Rp ", payment.pi.Amount),
                        Branch = payment.b.Name
                    });
                }
            }

            if (list == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }
    }
}
