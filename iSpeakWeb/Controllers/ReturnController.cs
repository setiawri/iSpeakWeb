using iSpeak.Common;
using iSpeak.Models;
using Newtonsoft.Json;
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
    public class ReturnController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region GET DATA INDEX
        public async Task<JsonResult> GetData()
        {
            var items = await db.SaleReturns.ToListAsync();
            List<SaleReturnViewModels> list = new List<SaleReturnViewModels>();
            foreach (var item in items)
            {
                list.Add(new SaleReturnViewModels
                {
                    Id = item.Id,
                    No = item.No,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Notes = item.Notes,
                    Approved = item.Approved
                });
            }

            return Json(new { data = list });
        }
        #endregion
        #region GET INVOICE NUMBER
        public async Task<JsonResult> GetInvoiceNo(string search, int page, int limit, Guid branch_id)
        {
            int offset = limit * (page - 1);
            List<SaleInvoicesIndexModels> list = new List<SaleInvoicesIndexModels>();
            if (string.IsNullOrEmpty(search))
            {
                list = await (from si in db.SaleInvoices
                              join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                              where si.Due == 0 && si.Cancelled == false && si.Branches_Id == branch_id
                              orderby si.No
                              select new SaleInvoicesIndexModels
                              {
                                  Id = si.Id,
                                  No = si.No,
                                  Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname
                              }).Skip(offset).Take(limit).ToListAsync();
            }
            else
            {
                list = await (from si in db.SaleInvoices
                              join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                              where si.Due == 0 && si.Cancelled == false && si.Branches_Id == branch_id
                                && (si.No.Contains(search) || u.Firstname.Contains(search) || u.Middlename.Contains(search) || u.Lastname.Contains(search))
                              orderby si.No
                              select new SaleInvoicesIndexModels
                              {
                                  Id = si.Id,
                                  No = si.No,
                                  Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname
                              }).Skip(offset).Take(limit).ToListAsync();
            }

            List<Select2Pagination.Select2Results> results = new List<Select2Pagination.Select2Results>();
            foreach (var item in list)
            {
                results.Add(new Select2Pagination.Select2Results
                {
                    id = item.Id.ToString(),
                    text = string.Format("{0} - {1}", item.No, item.Customer)
                });
            }

            Select2Pagination.Select2Page pagination = new Select2Pagination.Select2Page
            {
                more = results.Count() == limit ? true : false
            };

            return Json(new { results, pagination }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GET INVENTORY DATA
        public async Task<JsonResult> GetInventoryData(Guid invoice_id)
        {
            string body = "";
            var inventories = await db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == invoice_id && x.Products_Id != null).OrderBy(x => x.RowNo).ToListAsync();
            if (inventories.Count > 0)
            {
                foreach (var item in inventories)
                {
                    int qty_returned = db.SaleReturnItems.Where(x => x.SaleInvoiceItems_Id == item.Id).Sum(x => (int?)x.Qty) ?? 0;
                    body += "<tr>";
                    body += "<td><input type='text' readonly class='form-control desc' value='" + item.Description + "' /><input type='hidden' readonly class='form-control inv_item_id' value='" + item.Id + "' /></td>";
                    body += "<td><input type='text' readonly class='form-control qty_inv' value='" + item.Qty + "' /></td>";
                    body += "<td><input type='text' readonly class='form-control qty_returned' value='" + qty_returned + "' /></td>";
                    body += "<td><input type='number' class='form-control qty_return' value='0' min='0' max='" + (item.Qty - qty_returned) + "' onkeyup='if(parseInt(this.value)>" + (item.Qty - qty_returned) + "){this.value=" + (item.Qty - qty_returned) + "; return false;}' /></td>";
                    //body += "<td><input type='hidden' readonly class='form-control inv_item_id' value='" + item.Id + "' /></td>";
                    body += "</tr>";
                }
            }
            return Json(new { body }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GET ITEM
        public async Task<JsonResult> GetItem(Guid id)
        {
            var list = await (from sr in db.SaleReturns
                              join sri in db.SaleReturnItems on sr.Id equals sri.SaleReturns_Id
                              join sii in db.SaleInvoiceItems on sri.SaleInvoiceItems_Id equals sii.Id
                              join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                              where sr.Id == id
                              select new { sr, sri, sii, si }).ToListAsync();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>No. Invoice</th>
                                                <th>Description</th>
                                                <th>Qty</th>
                                                <th>Price</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>" + item.si.No + @"</td>
                                <td>" + item.sii.Description + @"</td>
                                <td>" + item.sri.Qty + @"</td>
                                <td>" + string.Format("{0:N0}", item.sii.Price) + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region APPROVE
        public async Task<JsonResult> Approved(Guid id)
        {
            var sale_return = await db.SaleReturns.FindAsync(id);
            sale_return.Approved = true;
            db.Entry(sale_return).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region CANCEL APPROVE
        public async Task<JsonResult> CancelApproved(Guid id)
        {
            var sale_return = await db.SaleReturns.FindAsync(id);
            sale_return.Approved = false;
            db.Entry(sale_return).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Approve = p.IsGranted(User.Identity.Name, "return_approve");
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View();
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,No,Timestamp,Notes,Approved")] SaleReturnsModels saleReturnsModels, string Items)
        {
            if (Items == "[]")
            {
                ModelState.AddModelError("Invoice_Id", "The field No. Invoice is required.");
            }

            if (ModelState.IsValid)
            {
                string lastHex_string = db.SaleReturns.AsNoTracking().Max(x => x.No);
                int lastHex_int = int.Parse(
                    string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                    System.Globalization.NumberStyles.HexNumber);

                saleReturnsModels.Id = Guid.NewGuid();
                saleReturnsModels.No = (lastHex_int + 1).ToString("X5");
                saleReturnsModels.Timestamp = DateTime.UtcNow;
                saleReturnsModels.Approved = false;
                db.SaleReturns.Add(saleReturnsModels);

                var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
                List<SaleReturnDetails> list_item = JsonConvert.DeserializeObject<List<SaleReturnDetails>>(Items);
                foreach (var item in list_item)
                {
                    if (item.qty_return > 0)
                    {
                        SaleReturnItemsModels saleReturnItemsModels = new SaleReturnItemsModels
                        {
                            Id = Guid.NewGuid(),
                            SaleReturns_Id = saleReturnsModels.Id,
                            SaleInvoiceItems_Id = item.inv_item_id,
                            Qty = item.qty_return
                        };
                        db.SaleReturnItems.Add(saleReturnItemsModels);

                        var sale_invoice_item = await db.SaleInvoiceItems.FindAsync(item.inv_item_id);
                        var product_qty = await db.Products_Qty.Where(x => x.Branches_Id == user_login.Branches_Id && x.Products_Id == sale_invoice_item.Products_Id).FirstOrDefaultAsync();
                        product_qty.Qty += item.qty_return;
                        db.Entry(product_qty).State = EntityState.Modified;

                        var sii_inventory = await (from si in db.SaleInvoiceItems_Inventory
                                                   join i in db.Inventory on si.Inventory_Id equals i.Id
                                                   where si.SaleInvoiceItems_Id == sale_invoice_item.Id && i.Products_Id == sale_invoice_item.Products_Id
                                                   orderby i.ReceiveDate
                                                   select new { si, i }).ToListAsync();
                        foreach (var subitem in sii_inventory)
                        {
                            var inventory = await db.Inventory.FindAsync(subitem.i.Id);
                            inventory.AvailableQty += subitem.si.Qty;
                            db.Entry(inventory).State = EntityState.Modified;
                        }
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(saleReturnsModels);
        }
    }
}