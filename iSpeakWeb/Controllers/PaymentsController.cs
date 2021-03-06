﻿using iSpeak.Common;
using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        //public async Task<ActionResult> Index()
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        var data = (from si in db.SaleInvoices
        //                    join b in db.Branches on si.Branches_Id equals b.Id
        //                    join u in db.User on si.Customer_UserAccounts_Id equals u.Id
        //                    //where si.Due > 0
        //                    select new SaleInvoicesIndexModels
        //                    {
        //                        Id = si.Id,
        //                        Branches = b.Name,
        //                        No = si.No,
        //                        Timestamp = si.Timestamp,
        //                        Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname,
        //                        Amount = si.Amount,
        //                        Due = si.Due,
        //                        Cancelled = si.Cancelled
        //                    }).ToListAsync();
        //        return View(await data);
        //    }
        //}

        #region Get Data
        public async Task<JsonResult> GetData()
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            List<PaymentsIndexModels> list = new List<PaymentsIndexModels>();

            //NEW Code
            List<PaymentIndexSQL> items = new List<PaymentIndexSQL>();
            using (var ctx = new iSpeakContext())
            {
                items = ctx.Database.SqlQuery<PaymentIndexSQL>(@"
                    SELECT 
                    p.Id
                    ,(SELECT Name FROM Branches WHERE Id='" + user_login.Branches_Id + @"') Branch
                    ,p.Timestamp Date,p.No,p.CashAmount Cash,p.DebitAmount Debit,p.ConsignmentAmount Consignment,p.Cancelled,p.Confirmed,p.Notes_Cancel
                    ,CASE WHEN y.Payments_Id IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END Has_Session
                    FROM Payments p
                    INNER JOIN (
	                    SELECT p.Id
	                    FROM Payments p
	                    INNER JOIN PaymentItems pi ON p.Id=pi.Payments_Id
	                    INNER JOIN SaleInvoices si ON pi.ReferenceId=si.Id
	                    INNER JOIN Branches b ON si.Branches_Id=b.Id
	                    WHERE b.Id='" + user_login.Branches_Id + @"'
	                    GROUP BY p.Id
                    ) x ON p.Id=x.Id
                    LEFT JOIN (
	                    SELECT pi.Payments_Id
	                    FROM PaymentItems pi
	                    INNER JOIN SaleInvoices si ON pi.ReferenceId=si.Id
	                    INNER JOIN SaleInvoiceItems sii ON si.Id=sii.SaleInvoices_Id
	                    WHERE sii.SessionHours > sii.SessionHours_Remaining
	                    GROUP BY pi.Payments_Id
                    ) y ON p.Id=y.Payments_Id
                    ORDER BY p.Timestamp DESC
                ").ToList();
            }

            foreach (var item in items)
            {
                list.Add(new PaymentsIndexModels
                {
                    Id = item.Id,
                    Branch = item.Branch,
                    No = item.No,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.Date, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    CashAmount = item.Cash,
                    DebitAmount = item.Debit,
                    ConsignmentAmount = item.Consignment,
                    Cancelled = item.Cancelled,
                    Confirmed = item.Confirmed,
                    Notes_Cancel = item.Notes_Cancel,
                    HasSession = item.Has_Session
                });
            }

            //OLD Code
            //foreach (var item in await db.Payments.ToListAsync())
            //{
            //    var results = await (from pi in db.PaymentItems
            //                         join si in db.SaleInvoices on pi.ReferenceId equals si.Id
            //                         join b in db.Branches on si.Branches_Id equals b.Id
            //                         where pi.Payments_Id == item.Id
            //                         select new { si, b }).FirstOrDefaultAsync();
            //    if (user_login.Branches_Id == results.si.Branches_Id)
            //    {

            //        var check_session = await (from pi in db.PaymentItems
            //                                   join si in db.SaleInvoices on pi.ReferenceId equals si.Id
            //                                   join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
            //                                   where pi.Payments_Id == item.Id && sii.SessionHours > sii.SessionHours_Remaining
            //                                   select new { pi }).ToListAsync();

            //        PaymentsIndexModels pim = new PaymentsIndexModels
            //        {
            //            Id = item.Id,
            //            Branch = results.b.Name,
            //            No = item.No,
            //            Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
            //            CashAmount = item.CashAmount,
            //            DebitAmount = item.DebitAmount,
            //            ConsignmentAmount = item.ConsignmentAmount,
            //            Cancelled = item.Cancelled,
            //            Confirmed = item.Confirmed,
            //            Notes_Cancel = item.Notes_Cancel,
            //            HasSession = check_session.Count > 0 ? true : false
            //            //Action = item.Notes_Cancel
            //        };
            //        list.Add(pim);
            //    }
            //    //Guid sales_invoice_id = db.PaymentItems.Where(x => x.Payments_Id == item.Id).FirstOrDefault().ReferenceId;
            //    //Guid branch_id = db.SaleInvoices.Where(x => x.Id == sales_invoice_id).FirstOrDefault().Branches_Id;
            //    //pim.Branch = db.Branches.Where(x => x.Id == branch_id).FirstOrDefault().Name;
            //    //if (branch_id == user_branch)
            //    //    list.Add(pim);
            //}

            int totalRows = list.Count;

            //Server Side Parameters
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            //Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list
                    .Where(x => x.Branch.ToLower().Contains(searchValue.ToLower())
                        || x.Timestamp.ToLower().Contains(searchValue.ToLower())
                        || x.No.ToLower().Contains(searchValue.ToLower())
                        || x.CashAmount.ToString().Contains(searchValue.ToLower())
                        || x.DebitAmount.ToString().Contains(searchValue.ToLower())
                        || x.ConsignmentAmount.ToString().Contains(searchValue.ToLower())
                    ).ToList();
            }
            int totalRowsFiltered = list.Count;

            //Sorting
            list = list.OrderBy(sortColumnName + " " + sortDirection).ToList();

            //Paging
            list = list.Skip(start).Take(length).ToList();

            return Json(new { data = list, draw = Request["draw"], recordsTotal = totalRows, recordsFiltered = totalRowsFiltered }, JsonRequestBehavior.AllowGet);

            //return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Payment
        public JsonResult GetPayment(Guid id)
        {
            var list = (from pi in db.PaymentItems
                        join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                        join p in db.Payments on pi.Payments_Id equals p.Id
                        where si.Id == id //&& p.Cancelled == false
                        orderby p.Timestamp descending
                        select new { pi, si, p }).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Payment No.</th>
                                                <th>Timestamp</th>
                                                <th>Due Before</th>
                                                <th>Payment</th>
                                                <th>Due After</th>
                                                <th>Status</th>
                                                <th>Approved</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            Permission permission = new Permission();
            bool canApprove = permission.IsGranted(User.Identity.Name, "payments_approve");
            foreach (var item in list)
            {
                string status = (item.p.Cancelled) 
                    ? "<span class='badge badge-warning d-block'>Cancel</span>" 
                    : "";

                string approved_render = "";
                if (canApprove)
                {
                    approved_render = item.p.Confirmed ? "<span class='badge badge-success d-block'>Approved</span>"
                        : "<a href='#'><span class='badge badge-dark d-block' onclick='Approve_Payment(\"" + item.p.Id + "\")'>None</span></a>";
                }
                message += @"<tr>
                                <td><a href='" + Url.Content("~") + "Payments/Print/"+ item.p.Id +"' target='_blank'>" + item.p.No + @"</a></td>
                                <td>" + string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.p.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))) + @"</td>
                                <td>" + item.pi.DueBefore.ToString("#,##0") + @"</td>
                                <td>" + item.pi.Amount.ToString("#,##0") + @"</td>
                                <td>" + item.pi.DueAfter.ToString("#,##0") + @"</td>
                                <td>" + status + @"</td>
                                <td>" + approved_render + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Payment
        public async Task<JsonResult> Cancelled(Guid id, string notes)
        {
            var payment = await db.Payments.FindAsync(id);
            payment.Cancelled = true;
            payment.Notes_Cancel = notes;
            db.Entry(payment).State = EntityState.Modified;

            var payment_item = await db.PaymentItems.Where(x => x.Payments_Id == id).ToListAsync();
            foreach (var item in payment_item)
            {
                var sale_invoice = await db.SaleInvoices.FindAsync(item.ReferenceId);
                sale_invoice.Due += item.Amount;
                db.Entry(sale_invoice).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Approve Payment
        public async Task<JsonResult> Approved(Guid id)
        {
            var payment = await db.Payments.FindAsync(id);
            payment.Confirmed = true;
            db.Entry(payment).State = EntityState.Modified;
            
            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Approve Payment
        public async Task<JsonResult> CancelApproved(Guid id)
        {
            var payment = await db.Payments.FindAsync(id);
            payment.Confirmed = false;
            db.Entry(payment).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

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
                        SaleInvoiceItemsDetails saleInvoiceItemsDetails = new SaleInvoiceItemsDetails
                        {
                            Invoice = db.SaleInvoices.Where(x => x.Id.ToString() == inv_id).FirstOrDefault().No,
                            Description = item.Description,
                            Notes = item.Notes,
                            Qty = item.Qty,
                            Price = item.Price,
                            Travel = item.TravelCost,
                            Tutor = item.TutorTravelCost,
                            Voucher = (item.SaleInvoiceItems_Vouchers_Id.HasValue) ? db.SaleInvoiceItems_Vouchers.Where(x => x.Id == item.SaleInvoiceItems_Vouchers_Id).FirstOrDefault().Amount : 0,
                            Discount = item.DiscountAmount
                        };
                        saleInvoiceItemsDetails.Amount = (item.Qty * item.Price) + item.TravelCost - item.DiscountAmount - saleInvoiceItemsDetails.Voucher;
                        listDetails.Add(saleInvoiceItemsDetails);
                        total += saleInvoiceItemsDetails.Amount;
                    }
                    due += db.SaleInvoices.Where(x => x.Id.ToString() == inv_id).FirstOrDefault().Due;
                }
                ViewBag.Total = total.ToString("#,##0");
                ViewBag.Due = due.ToString("#,##0");
                Guid branch_id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
                ViewBag.listConsignment = new SelectList(db.Consignments.Where(x => x.Branches_Id == branch_id).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.Invoices = id;
                return View(listDetails);
            }
        }

        public JsonResult SavePayments(Guid branch_id, int cash_amount, int consignment_amount, Guid? consignment_id, int bank_amount, string bank_name, string owner_name, string bank_number, string reff_no, string notes, string bank_type, string invoices_id)
        {
            string lastHex_string = db.Payments.AsNoTracking().Max(x => x.No);
            int lastHex_int = int.Parse(
                string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                System.Globalization.NumberStyles.HexNumber);

            PaymentsModels paymentsModels = new PaymentsModels
            {
                Id = Guid.NewGuid(),
                No = (lastHex_int + 1).ToString("X5"),
                Timestamp = DateTime.UtcNow,
                CashAmount = cash_amount,

                DebitAmount = bank_amount,
                DebitBank = (bank_amount == 0) ? "" : bank_name,
                DebitOwnerName = (bank_amount == 0) ? "" : owner_name,
                DebitNumber = (bank_amount == 0) ? "" : bank_number,
                DebitRefNo = (bank_amount == 0) ? "" : reff_no,
                Consignments_Id = (consignment_amount == 0) ? null : consignment_id,
                ConsignmentAmount = consignment_amount,
                Notes = notes,
                Cancelled = false,
                Confirmed = false,
                IsTransfer = (bank_type == "Transfer") ? true : false
            };

            string status; Guid payment_id;
            DateTime dateStart = DateTime.UtcNow.AddSeconds(-10); //range time = 10 seconds
            DateTime dateEnd = DateTime.UtcNow;
            var paymentCheck = db.Payments.Where(x =>
                                            x.Timestamp >= dateStart && x.Timestamp <= dateEnd
                                            && x.CashAmount == paymentsModels.CashAmount
                                            && x.DebitAmount == paymentsModels.DebitAmount
                                            && x.DebitBank == paymentsModels.DebitBank
                                            && x.DebitOwnerName == paymentsModels.DebitOwnerName
                                            && x.DebitNumber == paymentsModels.DebitNumber
                                            && x.DebitRefNo == paymentsModels.DebitRefNo
                                            && x.Consignments_Id == paymentsModels.Consignments_Id
                                            && x.ConsignmentAmount == paymentsModels.ConsignmentAmount
                                            && x.Notes == paymentsModels.Notes
                                            && x.Cancelled == paymentsModels.Cancelled
                                            && x.Confirmed == paymentsModels.Confirmed
                                            && x.IsTransfer == paymentsModels.IsTransfer
                                        ).FirstOrDefault();
            if (paymentCheck != null) { status = "300"; payment_id = paymentCheck.Id; } //duplicate
            else
            {
                status = "200"; payment_id = paymentsModels.Id;
                db.Payments.Add(paymentsModels);

                int total_paid = cash_amount + bank_amount + consignment_amount;
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

                        PaymentItemsModels paymentItemsModels = new PaymentItemsModels
                        {
                            Id = Guid.NewGuid(),
                            Payments_Id = paymentsModels.Id,
                            ReferenceId = saleInvoicesModels.Id,
                            Amount = (due_inv > saleInvoicesModels.Due) ? due_inv - saleInvoicesModels.Due : saleInvoicesModels.Due - due_inv,
                            DueBefore = due_inv,
                            DueAfter = saleInvoicesModels.Due
                        };
                        db.PaymentItems.Add(paymentItemsModels);
                    }
                }

                if (cash_amount > 0)
                {
                    string lastHex_string_pcr = db.PettyCashRecords.AsNoTracking().Max(x => x.No);
                    int lastHex_int_pcr = int.Parse(
                        string.IsNullOrEmpty(lastHex_string_pcr) ? 0.ToString("X5") : lastHex_string_pcr,
                        System.Globalization.NumberStyles.HexNumber);

                    PettyCashRecordsModels pettyCashRecordsModels = new PettyCashRecordsModels
                    {
                        Id = Guid.NewGuid(),
                        Branches_Id = branch_id,
                        RefId = paymentsModels.Id,
                        No = (lastHex_int_pcr + 1).ToString("X5"),
                        Timestamp = DateTime.UtcNow,
                        PettyCashRecordsCategories_Id = db.Settings.Where(x => x.Id == SettingsValue.GUID_AutoEntryForCashPayments).FirstOrDefault().Value_Guid.Value, //db.PettyCashRecordsCategories.Where(x => x.Name == "Penjualan Tunai").FirstOrDefault().Id,
                        Notes = "Cash Payment [" + paymentsModels.No + "]",
                        Amount = cash_amount,
                        IsChecked = false,
                        UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id
                    };
                    db.PettyCashRecords.Add(pettyCashRecordsModels);
                }

                db.SaveChanges();
            }

            return Json(new { status, payment_id }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Print(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                if (id == null || id == Guid.Empty) //show payment index
                {
                    //var login_session = Session["Login"] as LoginViewModel;
                    //Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
                    //List<PaymentsIndexModels> list_pim = new List<PaymentsIndexModels>();
                    //foreach (var item in db.Payments.ToList())
                    //{
                    //    var check_session = await (from pi in db.PaymentItems
                    //                               join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                    //                               join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                    //                               where pi.Payments_Id == item.Id && sii.SessionHours > sii.SessionHours_Remaining
                    //                               select new { pi, si, sii }).ToListAsync();

                    //    PaymentsIndexModels pim = new PaymentsIndexModels
                    //    {
                    //        Id = item.Id,
                    //        No = item.No,
                    //        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    //        CashAmount = item.CashAmount,
                    //        DebitAmount = item.DebitAmount,
                    //        ConsignmentAmount = item.ConsignmentAmount,
                    //        Cancelled = item.Cancelled,
                    //        Confirmed = item.Confirmed,
                    //        Notes_Cancel = item.Notes_Cancel,
                    //        HasSession = check_session.Count > 0 ? true : false
                    //    };
                    //    Guid sales_invoice_id = db.PaymentItems.Where(x => x.Payments_Id == item.Id).FirstOrDefault().ReferenceId;
                    //    Guid branch_id = db.SaleInvoices.Where(x => x.Id == sales_invoice_id).FirstOrDefault().Branches_Id;
                    //    pim.Branch = db.Branches.Where(x => x.Id == branch_id).FirstOrDefault().Name;
                    //    if (branch_id == user_branch)
                    //        list_pim.Add(pim);
                    //}

                    ViewBag.Cancel = p.IsGranted(User.Identity.Name, "payments_cancel");
                    ViewBag.Approve = p.IsGranted(User.Identity.Name, "payments_approve");
                    ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                    return View(); //return View(list_pim);
                }
                else //show payment receipt
                {
                    PaymentsModels paymentsModels = await db.Payments.Where(x => x.Id == id).FirstOrDefaultAsync();
                    BranchesModels branchesModels = new BranchesModels();
                    List<PaymentItemsDetails> listItems = new List<PaymentItemsDetails>();
                    List<SaleInvoiceItemsDetails> listDetails = new List<SaleInvoiceItemsDetails>();
                    decimal total_paid = 0;

                    //var list_PaymentItemsModels = db.PaymentItems.Where(x => x.Payments_Id == paymentsModels.Id).ToList();
                    var list_PaymentItemsModels = await (from pi in db.PaymentItems
                                                         join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                                                         where pi.Payments_Id == paymentsModels.Id
                                                         orderby si.Timestamp ascending
                                                         select new { pi }).ToListAsync();
                    foreach (var item in list_PaymentItemsModels)
                    {
                        Guid branch_id = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).FirstOrDefault().Branches_Id;
                        branchesModels = await db.Branches.Where(x => x.Id == branch_id).FirstOrDefaultAsync();
                        
                        PaymentItemsDetails paymentItemsDetails = new PaymentItemsDetails
                        {
                            Invoice = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).FirstOrDefault().No,
                            Amount = db.SaleInvoices.Where(x => x.Id == item.pi.ReferenceId).Sum(x => x.Amount),
                            DueBefore = item.pi.DueBefore,
                            Payment = (item.pi.DueBefore > item.pi.DueAfter) ? item.pi.DueBefore - item.pi.DueAfter : item.pi.DueAfter - item.pi.DueBefore,
                            DueAfter = item.pi.DueAfter
                        };
                        listItems.Add(paymentItemsDetails);
                        total_paid += paymentItemsDetails.Payment;

                        //decimal total = 0;
                        var list_SaleInvoiceItemsModels = await db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == item.pi.ReferenceId).OrderBy(x => x.RowNo).ToListAsync();
                        foreach (var subitem in list_SaleInvoiceItemsModels)
                        {
                            var data_customer = await (from si in db.SaleInvoices
                                                       join c in db.User on si.Customer_UserAccounts_Id equals c.Id
                                                       where si.Id == item.pi.ReferenceId
                                                       select new { c }).FirstOrDefaultAsync();
                            SaleInvoiceItemsDetails saleInvoiceItemsDetails = new SaleInvoiceItemsDetails();
                            saleInvoiceItemsDetails.Invoice = paymentItemsDetails.Invoice;
                            saleInvoiceItemsDetails.Description = subitem.Description;
                            saleInvoiceItemsDetails.Notes = subitem.Notes;
                            saleInvoiceItemsDetails.Customer = data_customer.c.Firstname + " " + data_customer.c.Middlename + " " + data_customer.c.Lastname;
                            saleInvoiceItemsDetails.Qty = subitem.Qty;
                            saleInvoiceItemsDetails.Price = subitem.Price;
                            saleInvoiceItemsDetails.Travel = subitem.TravelCost;
                            saleInvoiceItemsDetails.Tutor = subitem.TutorTravelCost;
                            saleInvoiceItemsDetails.Voucher = (subitem.SaleInvoiceItems_Vouchers_Id.HasValue) ? db.SaleInvoiceItems_Vouchers.Where(x => x.Id == subitem.SaleInvoiceItems_Vouchers_Id).FirstOrDefault().Amount : 0;
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
                    //receiptViewModels.TotalCash = paymentsModels.CashAmount;
                    //receiptViewModels.TotalDebit = paymentsModels.DebitAmount;
                    receiptViewModels.ConsignmentName = (paymentsModels.Consignments_Id.HasValue) ? db.Consignments.Where(x => x.Id == paymentsModels.Consignments_Id.Value).FirstOrDefault().Name : "";
                    receiptViewModels.TotalAmount = total_paid;
                    return View("Printed", receiptViewModels);
                }
            }
        }
    }
}