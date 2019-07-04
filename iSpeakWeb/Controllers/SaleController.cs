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
    public class SaleController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public JsonResult GetItem(Guid id)
        {
            var list = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == id).OrderBy(x => x.RowNo).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Description</th>
                                                <th>Qty</th>
                                                <th>Price</th>
                                                <th>Discount</th>
                                                <th>Voucher</th>
                                                <th>Subtotal</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                decimal voucher = (item.Vouchers_Id.HasValue) ? db.Vouchers.Where(x => x.Id == item.Vouchers_Id).FirstOrDefault().Amount : 0;
                decimal subtotal = (item.Qty * item.Price) - item.DiscountAmount - voucher;
                message += @"<tr>
                                <td>" + item.Description + @"</td>
                                <td>" + item.Qty + @"</td>
                                <td>" + item.Price.ToString("#,##0") + @"</td>
                                <td>" + item.DiscountAmount.ToString("#,##0") + @"</td>
                                <td>" + voucher.ToString("#,##0") + @"</td>
                                <td>" + subtotal.ToString("#,##0") + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Index()
        {
            var data = (from si in db.SaleInvoices
                        join b in db.Branches on si.Branches_Id equals b.Id
                        join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                        select new SaleInvoicesIndexModels
                        {
                            Id = si.Id,
                            Branches = b.Name,
                            No = si.No,
                            Timestamp = si.Timestamp,
                            Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Amount = si.Amount,
                            Cancelled = si.Cancelled
                        }).ToListAsync();
            return View(await data);
        }

        public JsonResult GetItemTotal(int qty, Guid lesson_package_id, int travel, decimal disc, string voucher_id)
        {
            var lessonPackage = db.LessonPackages.Where(x => x.Id == lesson_package_id).FirstOrDefault();
            string description = lessonPackage.Name;
            int price = lessonPackage.Price;
            decimal voucher = string.IsNullOrEmpty(voucher_id) ? 0 : db.Vouchers.Where(x => x.Id.ToString() == voucher_id).FirstOrDefault().Amount;
            decimal subtotal = (qty * price) + travel - disc - voucher;

            return Json(new { description = description, price = price, voucher = voucher, subtotal = subtotal }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listVoucher = new SelectList(db.Vouchers.Where(x => x.Active == true).OrderBy(x => x.Code).ToList(), "Id", "Code");
            var customers = (from u in db.User
                            join ur in db.UserRole on u.Id equals ur.UserId
                            join r in db.Role on ur.RoleId equals r.Id
                            where r.Name == "Student"
                            orderby u.Firstname
                            select new { u }).ToList();
            List<object> customer_list = new List<object>();
            foreach (var item in customers)
            {
                customer_list.Add(new
                {
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listCustomer = new SelectList(customer_list, "Id", "Name");
            var lessons = (from lp in db.LessonPackages
                           join l in db.Languages on lp.Languages_Id equals l.Id
                           join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                           select new LessonPackagesViewModels
                           {
                               Id = lp.Id,
                               Name = lp.Name,
                               Languages = l.Name,
                               LessonTypes = lt.Name,
                               SessionHours = lp.SessionHours,
                               ExpirationDay = lp.ExpirationDay,
                               Price = lp.Price,
                               Active = lp.Active
                           }).ToList();
            List<object> lesson_list = new List<object>();
            foreach (var item in lessons)
            {
                lesson_list.Add(new
                {
                    Id = item.Id,
                    Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + " days, " + item.Price.ToString("#,##0") + ")"
                });
            }
            ViewBag.listLesson = new SelectList(lesson_list, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Branches_Id,LessonPackages_Id,Vouchers_Id,Customers_Id,Notes")] SaleInvoicesViewModels saleInvoicesViewModels, int Amount, string Items)
        {
            if (ModelState.IsValid)
            {
                string lastHex_string = db.SaleInvoices.AsNoTracking().Max(x => x.No);
                int lastHex_int = int.Parse(
                    string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                    System.Globalization.NumberStyles.HexNumber);

                SaleInvoicesModels saleInvoicesModels = new SaleInvoicesModels();
                saleInvoicesModels.Id = Guid.NewGuid();
                saleInvoicesModels.Branches_Id = saleInvoicesViewModels.Branches_Id; ;
                saleInvoicesModels.No = (lastHex_int + 1).ToString("X5");
                saleInvoicesModels.Timestamp = DateTime.Now;
                saleInvoicesModels.Customer_UserAccounts_Id = saleInvoicesViewModels.Customers_Id.ToString();
                saleInvoicesModels.Notes = saleInvoicesViewModels.Notes;
                saleInvoicesModels.Amount = Amount;
                saleInvoicesModels.Due = Amount;
                saleInvoicesModels.Cancelled = false;
                db.SaleInvoices.Add(saleInvoicesModels);

                byte row = 1;
                List<SaleInvoiceItemDetails> details = JsonConvert.DeserializeObject<List<SaleInvoiceItemDetails>>(Items);
                foreach (var item in details)
                {
                    SaleInvoiceItemsModels sii = new SaleInvoiceItemsModels();
                    sii.Id = Guid.NewGuid();
                    sii.RowNo = row;
                    sii.SaleInvoices_Id = saleInvoicesModels.Id;
                    sii.Description = item.desc;
                    sii.Qty = item.qty;
                    sii.Price = item.price;
                    sii.DiscountAmount = item.disc;
                    sii.Vouchers_Id = item.voucher_id;
                    sii.Notes = item.note;
                    sii.Products_Id = item.inventory_id;
                    sii.Services_Id = item.service_id;
                    sii.LessonPackages_Id = item.lesson_id;
                    sii.SessionHours = db.LessonPackages.Where(x => x.Id == item.lesson_id).FirstOrDefault().SessionHours;
                    sii.TravelCost = item.travel;
                    sii.TutorTravelCost = item.tutor;
                    db.SaleInvoiceItems.Add(sii);

                    row++;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listVoucher = new SelectList(db.Vouchers.Where(x => x.Active == true).OrderBy(x => x.Code).ToList(), "Id", "Code");
            var customers = (from u in db.User
                             join ur in db.UserRole on u.Id equals ur.UserId
                             join r in db.Role on ur.RoleId equals r.Id
                             where r.Name == "Student"
                             orderby u.Firstname
                             select new { u }).ToList();
            List<object> customer_list = new List<object>();
            foreach (var item in customers)
            {
                customer_list.Add(new
                {
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listCustomer = new SelectList(customer_list, "Id", "Name");
            var lessons = (from lp in db.LessonPackages
                           join l in db.Languages on lp.Languages_Id equals l.Id
                           join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                           select new LessonPackagesViewModels
                           {
                               Id = lp.Id,
                               Name = lp.Name,
                               Languages = l.Name,
                               LessonTypes = lt.Name,
                               SessionHours = lp.SessionHours,
                               Price = lp.Price,
                               Active = lp.Active
                           }).ToList();
            List<object> lesson_list = new List<object>();
            foreach (var item in lessons)
            {
                lesson_list.Add(new
                {
                    Id = item.Id,
                    Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + "days, " + item.Price.ToString("#,##0") + ")"
                });
            }
            ViewBag.listLesson = new SelectList(lesson_list, "Id", "Name");

            return View(saleInvoicesViewModels);
        }
        
    }
}