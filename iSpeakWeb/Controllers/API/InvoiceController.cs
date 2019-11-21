using iSpeak.Models;
using iSpeak.Models.API;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace iSpeak.Controllers.API
{
    public class InvoiceController : ApiController
    {
        private readonly iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/invoice")]
        public async Task<HttpResponseMessage> Invoices(CommonRequestModels model)
        {
            var invoices = await (from si in db.SaleInvoices
                                  join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                                  join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                                  where si.Cancelled == false && u.UserName == model.Username && sii.LessonPackages_Id != null
                                  orderby si.Timestamp descending
                                  select new { si, sii, u }).ToListAsync();
            List<InvoiceApiModels> list = new List<InvoiceApiModels>();
            if (invoices.Count > 0)
            {
                foreach (var invoice in invoices)
                {
                    string package_name;
                    if (invoice.sii.LessonPackages_Id.HasValue)
                    {
                        var a = await db.LessonPackages.Where(x => x.Id == invoice.sii.LessonPackages_Id).FirstOrDefaultAsync();
                        package_name = a.Name;
                    }
                    else if (invoice.sii.Products_Id.HasValue)
                    {
                        var a = await db.Products.Where(x => x.Id == invoice.sii.Products_Id).FirstOrDefaultAsync();
                        package_name = a.Description;
                    }
                    else if (invoice.sii.Services_Id.HasValue)
                    {
                        var a = await db.Services.Where(x => x.Id == invoice.sii.Services_Id).FirstOrDefaultAsync();
                        package_name = a.Description;
                    }
                    else { package_name = string.Empty; }

                    list.Add(new InvoiceApiModels
                    {
                        SaleInvoiceItems_Id = invoice.sii.Id,
                        No = "Invoice No. " + invoice.si.No,
                        Package = package_name,
                        Price = string.Format("{0} {1:N0}", "Rp", invoice.sii.Price),
                        Due = string.Format("{0} {1:N0}", "Rp", invoice.si.Due),
                        RemainingHours = string.Format("Avail. {0} of {1} hours", invoice.sii.SessionHours_Remaining.Value, invoice.sii.SessionHours.Value),
                        Status = invoice.si.Due > 0 ? "Waiting Payment" : "Payment Completed"
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
        public async Task<HttpResponseMessage> CheckPayments(CommonRequestModels model)
        {
            var payments = await (from p in db.Payments
                                  join pi in db.PaymentItems on p.Id equals pi.Payments_Id
                                  join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                                  join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                                  join b in db.Branches on si.Branches_Id equals b.Id
                                  where p.Cancelled == false && u.UserName == model.Username
                                  select new { p, pi, si, u, b }).ToListAsync();
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
                        Branch = payment.b.Name,
                        NoInvoice = "Invoice No. " + payment.si.No
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
