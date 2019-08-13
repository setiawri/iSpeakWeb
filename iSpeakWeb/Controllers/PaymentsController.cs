using iSpeak.Common;
using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var data = (from si in db.SaleInvoices
                            join b in db.Branches on si.Branches_Id equals b.Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            //where si.Due > 0
                            select new SaleInvoicesIndexModels
                            {
                                Id = si.Id,
                                Branches = b.Name,
                                No = si.No,
                                Timestamp = si.Timestamp,
                                Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                Amount = si.Amount,
                                Due = si.Due,
                                Cancelled = si.Cancelled
                            }).ToListAsync();
                return View(await data);
            }
        }

        public JsonResult GetPayment(Guid id)
        {
            var list = (from pi in db.PaymentItems
                        join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                        join p in db.Payments on pi.Payments_Id equals p.Id
                        where si.Id == id
                        orderby p.Timestamp descending
                        select new { pi, si, p }).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Invoice No.</th>
                                                <th>Timestamp</th>
                                                <th>Due Before</th>
                                                <th>Payment</th>
                                                <th>Due After</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>" + item.p.No + @"</td>
                                <td>" + item.p.Timestamp.ToString("yyyy/MM/dd HH:mm") + @"</td>
                                <td>" + item.pi.DueBefore.ToString("#,##0") + @"</td>
                                <td>" + item.pi.Amount.ToString("#,##0") + @"</td>
                                <td>" + item.pi.DueAfter.ToString("#,##0") + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Create(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                decimal total = 0; decimal due = 0;
                List<SaleInvoiceItemsDetails> listDetails = new List<SaleInvoiceItemsDetails>();
                string[] ids = id.Split(',');
                foreach (string inv_id in ids)
                {
                    var list = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id.ToString() == inv_id).OrderBy(x => x.RowNo).ToList();
                    foreach (var item in list)
                    {
                        SaleInvoiceItemsDetails saleInvoiceItemsDetails = new SaleInvoiceItemsDetails();
                        saleInvoiceItemsDetails.Invoice = db.SaleInvoices.Where(x => x.Id.ToString() == inv_id).FirstOrDefault().No;
                        saleInvoiceItemsDetails.Description = item.Description;
                        saleInvoiceItemsDetails.Qty = item.Qty;
                        saleInvoiceItemsDetails.Price = item.Price;
                        saleInvoiceItemsDetails.Travel = item.TravelCost;
                        saleInvoiceItemsDetails.Tutor = item.TutorTravelCost;
                        saleInvoiceItemsDetails.Voucher = (item.Vouchers_Id.HasValue) ? db.Vouchers.Where(x => x.Id == item.Vouchers_Id).FirstOrDefault().Amount : 0;
                        saleInvoiceItemsDetails.Discount = item.DiscountAmount;
                        saleInvoiceItemsDetails.Amount = (item.Qty * item.Price) + item.TravelCost - item.DiscountAmount - saleInvoiceItemsDetails.Voucher;
                        listDetails.Add(saleInvoiceItemsDetails);
                        total += saleInvoiceItemsDetails.Amount;
                    }
                    due += db.SaleInvoices.Where(x => x.Id.ToString() == inv_id).FirstOrDefault().Due;
                }
                ViewBag.Total = total.ToString("#,##0");
                ViewBag.Due = due.ToString("#,##0");
                ViewBag.Invoices = id;
                return View(listDetails);
            }
        }

        public JsonResult SavePayments(int cash_amount, int bank_amount, string bank_name, string owner_name, string bank_number, string reff_no, string notes, string bank_type, string invoices_id)
        {
            string lastHex_string = db.Payments.AsNoTracking().Max(x => x.No);
            int lastHex_int = int.Parse(
                string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                System.Globalization.NumberStyles.HexNumber);

            PaymentsModels paymentsModels = new PaymentsModels();
            paymentsModels.Id = Guid.NewGuid();
            paymentsModels.No = (lastHex_int + 1).ToString("X5");
            paymentsModels.Timestamp = DateTime.Now;
            paymentsModels.CashAmount = cash_amount;
            paymentsModels.DebitAmount = bank_amount;
            paymentsModels.DebitBank = (bank_amount == 0) ? "" : bank_name;
            paymentsModels.DebitOwnerName = (bank_amount == 0) ? "" : owner_name;
            paymentsModels.DebitNumber = (bank_amount == 0) ? "" : bank_number;
            paymentsModels.DebitRefNo = (bank_amount == 0) ? "" : reff_no;
            paymentsModels.Notes = notes;
            paymentsModels.Cancelled = false;
            paymentsModels.Confirmed = false;
            paymentsModels.IsTransfer = (bank_type == "Transfer") ? true : false;
            db.Payments.Add(paymentsModels);

            int total_paid = cash_amount + bank_amount;
            string[] ids = invoices_id.Split(',');
            for (int i = ids.Length - 1; i >= 0; i--)  //foreach (string id in ids)
            {
                if (total_paid > 0)
                {
                    string id_saleinvoice = ids[i];
                    SaleInvoicesModels saleInvoicesModels = db.SaleInvoices.Where(x => x.Id.ToString() == id_saleinvoice).FirstOrDefault();
                    int due_inv = saleInvoicesModels.Due;
                    if (total_paid >= due_inv)
                    {
                        saleInvoicesModels.Due = 0;
                        total_paid -= due_inv;
                    }
                    else
                    {
                        saleInvoicesModels.Due -= total_paid;
                        total_paid = 0;
                    }
                    db.Entry(saleInvoicesModels).State = EntityState.Modified;

                    PaymentItemsModels paymentItemsModels = new PaymentItemsModels();
                    paymentItemsModels.Id = Guid.NewGuid();
                    paymentItemsModels.Payments_Id = paymentsModels.Id;
                    paymentItemsModels.ReferenceId = saleInvoicesModels.Id;
                    paymentItemsModels.Amount = (due_inv > saleInvoicesModels.Due) ? due_inv - saleInvoicesModels.Due : saleInvoicesModels.Due - due_inv;
                    paymentItemsModels.DueBefore = due_inv;
                    paymentItemsModels.DueAfter = saleInvoicesModels.Due;
                    db.PaymentItems.Add(paymentItemsModels);
                }
            }

            db.SaveChanges();

            return Json(new { status = "200", payment_id = paymentsModels.Id }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Print(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                if (id == null || id == Guid.Empty)
                {
                    //var login_session = Session["Login"] as LoginViewModel;
                    Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
                    List<PaymentsIndexModels> list_pim = new List<PaymentsIndexModels>();
                    foreach (var item in db.Payments.ToList())
                    {
                        PaymentsIndexModels pim = new PaymentsIndexModels();
                        pim.Id = item.Id;
                        pim.No = item.No;
                        pim.Timestamp = item.Timestamp;
                        pim.CashAmount = item.CashAmount;
                        pim.DebitAmount = item.DebitAmount;
                        pim.Notes = item.Notes;
                        Guid sales_invoice_id = db.PaymentItems.Where(x => x.Payments_Id == item.Id).FirstOrDefault().ReferenceId;
                        Guid branch_id = db.SaleInvoices.Where(x => x.Id == sales_invoice_id).FirstOrDefault().Branches_Id;
                        pim.Branch = db.Branches.Where(x => x.Id == branch_id).FirstOrDefault().Name;
                        if (branch_id == user_branch)
                            list_pim.Add(pim);
                    }
                    return View(list_pim);
                }
                else
                {
                    PaymentsModels paymentsModels = db.Payments.Where(x => x.Id == id).FirstOrDefault();
                    BranchesModels branchesModels = new BranchesModels();
                    List<PaymentItemsDetails> listItems = new List<PaymentItemsDetails>();
                    List<SaleInvoiceItemsDetails> listDetails = new List<SaleInvoiceItemsDetails>();
                    decimal total_paid = 0;

                    //var list_PaymentItemsModels = db.PaymentItems.Where(x => x.Payments_Id == paymentsModels.Id).ToList();
                    var list_PaymentItemsModels = (from pi in db.PaymentItems
                                                   join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                                                   where pi.Payments_Id == paymentsModels.Id
                                                   orderby si.Timestamp ascending
                                                   select new { pi }).ToList();
                    foreach (var item in list_PaymentItemsModels)
                    {
                        Guid branch_id = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).FirstOrDefault().Branches_Id;
                        branchesModels = db.Branches.Where(x => x.Id == branch_id).FirstOrDefault();

                        PaymentItemsDetails paymentItemsDetails = new PaymentItemsDetails();
                        paymentItemsDetails.Invoice = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).FirstOrDefault().No;
                        paymentItemsDetails.Amount = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).Sum(x => x.Amount);
                        paymentItemsDetails.DueBefore = item.pi.DueBefore;
                        paymentItemsDetails.Payment = (item.pi.DueBefore > item.pi.DueAfter) ? item.pi.DueBefore - item.pi.DueAfter : item.pi.DueAfter - item.pi.DueBefore;
                        paymentItemsDetails.DueAfter = item.pi.DueAfter;
                        listItems.Add(paymentItemsDetails);
                        total_paid += paymentItemsDetails.Payment;

                        //decimal total = 0;
                        var list_SaleInvoiceItemsModels = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == item.pi.ReferenceId).OrderBy(x => x.RowNo).ToList();
                        foreach (var subitem in list_SaleInvoiceItemsModels)
                        {
                            SaleInvoiceItemsDetails saleInvoiceItemsDetails = new SaleInvoiceItemsDetails();
                            saleInvoiceItemsDetails.Invoice = paymentItemsDetails.Invoice;
                            saleInvoiceItemsDetails.Description = subitem.Description;
                            var data_customer = (from si in db.SaleInvoices
                                                 join c in db.User on si.Customer_UserAccounts_Id equals c.Id
                                                 where si.Id == item.pi.ReferenceId
                                                 select new { c }).FirstOrDefault();
                            saleInvoiceItemsDetails.Customer = data_customer.c.Firstname + " " + data_customer.c.Middlename + " " + data_customer.c.Lastname;
                            saleInvoiceItemsDetails.Qty = subitem.Qty;
                            saleInvoiceItemsDetails.Price = subitem.Price;
                            saleInvoiceItemsDetails.Travel = subitem.TravelCost;
                            saleInvoiceItemsDetails.Tutor = subitem.TutorTravelCost;
                            saleInvoiceItemsDetails.Voucher = (subitem.Vouchers_Id.HasValue) ? db.Vouchers.Where(x => x.Id == subitem.Vouchers_Id).FirstOrDefault().Amount : 0;
                            saleInvoiceItemsDetails.Discount = subitem.DiscountAmount;
                            saleInvoiceItemsDetails.Amount = (subitem.Qty * subitem.Price) + subitem.TravelCost - subitem.DiscountAmount - saleInvoiceItemsDetails.Voucher;
                            listDetails.Add(saleInvoiceItemsDetails);
                            //total += saleInvoiceItemsDetails.Amount;
                        }
                    }

                    ReceiptViewModels receiptViewModels = new ReceiptViewModels();
                    receiptViewModels.Branch = branchesModels;
                    receiptViewModels.Payment = paymentsModels;
                    receiptViewModels.listSaleInvoiceItems = listDetails;
                    receiptViewModels.listPaymentItems = listItems;
                    receiptViewModels.TotalCash = paymentsModels.CashAmount;
                    receiptViewModels.TotalDebit = paymentsModels.DebitAmount;
                    receiptViewModels.TotalAmount = total_paid;
                    return View("Printed", receiptViewModels);
                }
            }
        }
    }
}