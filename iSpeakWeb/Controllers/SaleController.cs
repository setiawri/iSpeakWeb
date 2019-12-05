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
    public class SaleController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region Get Data Index
        public async Task<JsonResult> GetData()
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            var data = await(from si in db.SaleInvoices
                             join b in db.Branches on si.Branches_Id equals b.Id
                             join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                             where si.Branches_Id == user_login.Branches_Id //login_session.Branches_Id
                             select new { si, b, u }).ToListAsync();

            List<SaleInvoicesIndexModels> list = new List<SaleInvoicesIndexModels>();
            foreach (var item in data)
            {
                list.Add(new SaleInvoicesIndexModels
                {
                    Id = item.si.Id,
                    Branches = item.b.Name,
                    No = item.si.No,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.si.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Customer = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                    Amount = item.si.Amount,
                    Due = item.si.Due,
                    Cancelled = item.si.Cancelled,
                    IsChecked = item.si.IsChecked,
                    Notes = item.si.Notes
                });
            }

            return Json(new { data = list });
        }
        #endregion
        #region Get Item
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
                                                <th>Travel</th>
                                                <th>Discount</th>
                                                <th>Voucher</th>
                                                <th>Subtotal</th>
                                                <th>Avail. Hours</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                decimal voucher = (item.SaleInvoiceItems_Vouchers_Id.HasValue) ? db.SaleInvoiceItems_Vouchers.Where(x => x.Id == item.SaleInvoiceItems_Vouchers_Id).FirstOrDefault().Amount : 0;
                decimal subtotal = (item.Qty * item.Price) + item.TravelCost - item.DiscountAmount - voucher;
                message += @"<tr>
                                <td>" + item.Description + "<br/>" + item.Notes + @"</td>
                                <td>" + item.Qty + @"</td>
                                <td>" + item.Price.ToString("#,##0") + @"</td>
                                <td>" + item.TravelCost.ToString("#,##0") + @"</td>
                                <td>" + item.DiscountAmount.ToString("#,##0") + @"</td>
                                <td>" + voucher.ToString("#,##0") + @"</td>
                                <td>" + subtotal.ToString("#,##0") + @"</td>
                                <td>" + item.SessionHours_Remaining + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Session Hours
        public async Task<JsonResult> GetSessionHours(Guid package_id)
        {
            var lesson_pckg = await db.LessonPackages.FindAsync(package_id);
            return Json(new { hours = string.Format("{0:N2}", lesson_pckg.SessionHours) }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Lesson Total
        public async Task<JsonResult> GetLessonTotal(int qty, Guid lesson_package_id, string hours, int travel, decimal disc, List<string> vouchers)
        {
            //var lessonPackage = db.LessonPackages.Where(x => x.Id == lesson_package_id).FirstOrDefault();
            var lesson = await (from lp in db.LessonPackages
                                 join l in db.Languages on lp.Languages_Id equals l.Id
                                 join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                                 where lp.Id == lesson_package_id
                                 select new { lp, l, lt }).FirstOrDefaultAsync();
            string description = "[" + lesson.lt.Name + ", " + lesson.l.Name + "] " + lesson.lp.Name + " (" + hours + " hours)";
            int price = lesson.lp.Price;
            //decimal voucher = string.IsNullOrEmpty(voucher_id) ? 0 : db.Vouchers.Where(x => x.Id.ToString() == voucher_id).FirstOrDefault().Amount;
            decimal voucher = 0; string voucher_selected = "";
            if (vouchers != null)
            {
                foreach (var v in vouchers)
                {
                    voucher += db.Vouchers.Where(x => x.Id.ToString() == v).FirstOrDefault().Amount;
                }
                voucher_selected = string.Join(",", vouchers);
            }
            decimal subtotal = (qty * price) + travel - disc - voucher;

            return Json(new { qty, description, price, voucher, subtotal, voucher_selected }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Inventory Total
        public async Task<JsonResult> GetInventoryTotal(int qty, Guid product_id, decimal disc, List<string> vouchers)
        {
            string error_message = ""; int price = 0; decimal voucher = 0; decimal subtotal = 0; string voucher_selected = "";
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            var product_qty = await db.Products_Qty.Where(x => x.Branches_Id == user_login.Branches_Id && x.Products_Id == product_id).FirstOrDefaultAsync();

            if (qty > product_qty.Qty)
            {
                error_message = "The Max Qty is " + string.Format("{0:N0}", product_qty.Qty) + ".";
            }
            else
            {
                price = db.Products.Where(x => x.Id == product_id).FirstOrDefault().SellPrice;
                //voucher = string.IsNullOrEmpty(voucher_id) ? 0 : db.Vouchers.Where(x => x.Id.ToString() == voucher_id).FirstOrDefault().Amount;
                if (vouchers != null)
                {
                    foreach (var v in vouchers)
                    {
                        voucher += db.Vouchers.Where(x => x.Id.ToString() == v).FirstOrDefault().Amount;
                    }
                    voucher_selected = string.Join(",", vouchers);
                }
                subtotal = (qty * price) - disc - voucher;
            }

            return Json(new { error_message, qty, price, voucher, subtotal, voucher_selected }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Service Total
        public async Task<JsonResult> GetServiceTotal(int qty, Guid service_id, decimal disc, List<string> vouchers)
        {
            string error_message = ""; int price = 0; decimal voucher = 0; decimal subtotal = 0; string voucher_selected = "";
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();

            price = db.Services.Where(x => x.Id == service_id).FirstOrDefault().SellPrice;
            //voucher = string.IsNullOrEmpty(voucher_id) ? 0 : db.Vouchers.Where(x => x.Id.ToString() == voucher_id).FirstOrDefault().Amount;
            if (vouchers != null)
            {
                foreach (var v in vouchers)
                {
                    voucher += db.Vouchers.Where(x => x.Id.ToString() == v).FirstOrDefault().Amount;
                }
                voucher_selected = string.Join(",", vouchers);
            }
            subtotal = (qty * price) - disc - voucher;

            return Json(new { error_message, qty, price, voucher, subtotal, voucher_selected }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Sale Invoice
        public async Task<JsonResult> Cancelled(Guid id, string notes)
        {
            var sale_invoice = await db.SaleInvoices.FindAsync(id);
            sale_invoice.Cancelled = true;
            sale_invoice.Notes = notes;
            db.Entry(sale_invoice).State = EntityState.Modified;

            var sale_invoice_items = await db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == id && x.Products_Id != null).ToListAsync();
            if (sale_invoice_items.Count > 0)
            {
                foreach (var item in sale_invoice_items)
                {
                    var products_qty = await db.Products_Qty.Where(x => x.Products_Id == item.Products_Id && x.Branches_Id == sale_invoice.Branches_Id).FirstOrDefaultAsync();
                    products_qty.Qty += item.Qty;
                    db.Entry(products_qty).State = EntityState.Modified;

                    var sii_inventory = await (from si in db.SaleInvoiceItems_Inventory
                                               join i in db.Inventory on si.Inventory_Id equals i.Id
                                               where si.SaleInvoiceItems_Id == item.Id && i.Products_Id == item.Products_Id
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
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Approve Sale Invoice
        public async Task<JsonResult> Approved(Guid id)
        {
            var sale_invoice = await db.SaleInvoices.FindAsync(id);
            sale_invoice.IsChecked = true;
            db.Entry(sale_invoice).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Approve Sale Invoice
        public async Task<JsonResult> CancelApproved(Guid id)
        {
            var sale_invoice = await db.SaleInvoices.FindAsync(id);
            sale_invoice.IsChecked = false;
            db.Entry(sale_invoice).State = EntityState.Modified;

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
                //var login_session = Session["Login"] as LoginViewModel;
                //var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
                //var data = await (from si in db.SaleInvoices
                //                  join b in db.Branches on si.Branches_Id equals b.Id
                //                  join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                //                  where si.Branches_Id == user_login.Branches_Id //login_session.Branches_Id
                //                  select new { si, b, u }).ToListAsync();

                //List<SaleInvoicesIndexModels> list = new List<SaleInvoicesIndexModels>();
                //foreach (var item in data)
                //{
                //    list.Add(new SaleInvoicesIndexModels
                //    {
                //        Id = item.si.Id,
                //        Branches = item.b.Name,
                //        No = item.si.No,
                //        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.si.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                //        Customer = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                //        Amount = item.si.Amount,
                //        Due = item.si.Due,
                //        Cancelled = item.si.Cancelled,
                //        IsChecked = item.si.IsChecked,
                //        Notes = item.si.Notes
                //    });
                //}

                ViewBag.Cancel = p.IsGranted(User.Identity.Name, "sale_cancel");
                ViewBag.Approve = p.IsGranted(User.Identity.Name, "sale_approve");
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View();
                //return View(list);
            }
        }
        
        [HttpGet]
        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var user_login = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                #region List Voucher
                var vouchers = db.Vouchers.Where(x => x.Active == true).OrderBy(x => x.Code).ToList();
                List<object> voucher_list = new List<object>();
                foreach (var item in vouchers)
                {
                    voucher_list.Add(new
                    {
                        item.Id,
                        Name = (string.IsNullOrEmpty(item.Notes))
                                ? "[" + item.Code + ": " + string.Format("{0:N2}", item.Amount) + "] " + item.Description
                                : "[" + item.Code + ": " + string.Format("{0:N2}", item.Amount) + "] " + item.Description + " (" + item.Notes + ")"
                    });
                }
                #endregion
                #region List Customer
                var customers = (from u in db.User
                                 where u.Active == true
                                 //join ur in db.UserRole on u.Id equals ur.UserId
                                 //join r in db.Role on ur.RoleId equals r.Id
                                 //r.Name == "Student"
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
                #endregion
                #region List Lesson
                var lessons = (from lp in db.LessonPackages
                               join l in db.Languages on lp.Languages_Id equals l.Id
                               join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                               where lp.Active == true
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
                        item.Id,
                        Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + string.Format("{0:N0}", item.Price) + ")"
                    });
                }
                #endregion
                #region List Product
                var list_product = (from pr in db.Products
                                    join pq in db.Products_Qty on pr.Id equals pq.Products_Id
                                    where pq.Branches_Id == user_login.Branches_Id
                                    select new { pr, pq }).ToList();
                List<object> products = new List<object>();
                foreach (var product in list_product)
                {
                    products.Add(new
                    {
                        product.pr.Id,
                        Name = product.pr.Description
                    });
                }
                #endregion
                #region List Role
                string role_id_allowed = db.Settings.Find(SettingsValue.GUID_UserSetRoleAllowed).Value_Guid.Value.ToString();
                List<SelectListItem> role_list = new List<SelectListItem>();
                bool setRole = p.IsGranted(User.Identity.Name, "user_setroles");
                if (setRole)
                {
                    foreach (var role in db.Role.OrderBy(x => x.Name))
                    {
                        role_list.Add(new SelectListItem() { Value = role.Name, Text = role.Name });
                    }
                }
                else
                {
                    foreach (var role in db.Role.Where(x => x.Id == role_id_allowed).OrderBy(x => x.Name))
                    {
                        role_list.Add(new SelectListItem() { Value = role.Name, Text = role.Name });
                    }
                }
                #endregion
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listVoucher = new SelectList(voucher_list, "Id", "Name");
                ViewBag.listCustomer = new SelectList(customer_list, "Id", "Name");
                ViewBag.listLesson = new SelectList(lesson_list, "Id", "Name");
                ViewBag.listProduct = new SelectList(products, "Id", "Name");
                ViewBag.listService = new SelectList(db.Services.Where(x => x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
                ViewBag.listRole = role_list;
                ViewBag.RoleValueDefault = db.Role.Find(role_id_allowed).Name;
                ViewBag.listLanguage = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listPromo = new SelectList(db.PromotionEvents.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.DOB = DateTime.UtcNow.Date;

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Branches_Id,LessonPackages_Id,Products_Id,Vouchers_Id,Customers_Id,Notes")] SaleInvoicesViewModels saleInvoicesViewModels, int Amount, string Items)
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                string lastHex_string = db.SaleInvoices.AsNoTracking().Max(x => x.No);
                int lastHex_int = int.Parse(
                    string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                    System.Globalization.NumberStyles.HexNumber);
                //var login_session = Session["Login"] as LoginViewModel;

                SaleInvoicesModels saleInvoicesModels = new SaleInvoicesModels
                {
                    Id = Guid.NewGuid(),
                    Branches_Id = user_login.Branches_Id, //login_session.Branches_Id;
                    No = (lastHex_int + 1).ToString("X5"),
                    Timestamp = DateTime.UtcNow,
                    Customer_UserAccounts_Id = saleInvoicesViewModels.Customers_Id.ToString(),
                    Notes = saleInvoicesViewModels.Notes,
                    Amount = Amount,
                    Due = Amount,
                    Cancelled = false,
                    IsChecked = false
                };
                db.SaleInvoices.Add(saleInvoicesModels);

                byte row = 1;
                List<SaleInvoiceItemDetails> details = JsonConvert.DeserializeObject<List<SaleInvoiceItemDetails>>(Items);
                foreach (var item in details)
                {
                    Guid? saleinvoiceitems_vouchers = null;
                    if (!string.IsNullOrEmpty(item.voucher_id))
                    {
                        SaleInvoiceItems_VouchersModels saleInvoiceItems_VouchersModels = new SaleInvoiceItems_VouchersModels
                        {
                            Id = Guid.NewGuid(),
                            Voucher_Ids = item.voucher_id,
                            Amount = item.voucher
                        };
                        db.SaleInvoiceItems_Vouchers.Add(saleInvoiceItems_VouchersModels);
                        saleinvoiceitems_vouchers = saleInvoiceItems_VouchersModels.Id;
                    }

                    SaleInvoiceItemsModels sii = new SaleInvoiceItemsModels
                    {
                        Id = Guid.NewGuid(),
                        RowNo = row,
                        SaleInvoices_Id = saleInvoicesModels.Id,
                        Description = item.desc,
                        Qty = item.qty,
                        Price = item.price,
                        DiscountAmount = item.disc,
                        SaleInvoiceItems_Vouchers_Id = saleinvoiceitems_vouchers, //item.voucher_id,
                        Notes = item.note,
                        Products_Id = item.inventory_id,
                        Services_Id = item.service_id,
                        LessonPackages_Id = item.lesson_id,
                        SessionHours = item.qty * item.hours, //item.lesson_id.HasValue ? db.LessonPackages.Where(x => x.Id == item.lesson_id).FirstOrDefault().SessionHours : 0,
                        SessionHours_Remaining = item.qty * item.hours, //item.lesson_id.HasValue ? db.LessonPackages.Where(x => x.Id == item.lesson_id).FirstOrDefault().SessionHours : 0,
                        TravelCost = item.travel,
                        TutorTravelCost = item.tutor
                    };
                    db.SaleInvoiceItems.Add(sii);

                    if (item.inventory_id.HasValue)
                    {
                        SyncSaleInvoice_Inventory(user_login.Branches_Id, item.inventory_id.Value, item.qty, sii.Id);

                        Products_QtyModels products_QtyModels = await db.Products_Qty.Where(x => x.Branches_Id == user_login.Branches_Id && x.Products_Id == item.inventory_id.Value).FirstOrDefaultAsync();
                        products_QtyModels.Qty -= item.qty;
                        db.Entry(products_QtyModels).State = EntityState.Modified;
                    }

                    row++;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            
            #region List Voucher
            var vouchers = db.Vouchers.Where(x => x.Active == true).OrderBy(x => x.Code).ToList();
            List<object> voucher_list = new List<object>();
            foreach (var item in vouchers)
            {
                voucher_list.Add(new
                {
                    item.Id,
                    Name = (string.IsNullOrEmpty(item.Notes))
                            ? "[" + item.Code + ": " + string.Format("{0:N2}", item.Amount) + "] " + item.Description
                            : "[" + item.Code + ": " + string.Format("{0:N2}", item.Amount) + "] " + item.Description + " (" + item.Notes + ")"
                });
            }
            #endregion
            #region List Customer
            var customers = (from u in db.User
                             where u.Active == true
                             //join ur in db.UserRole on u.Id equals ur.UserId
                             //join r in db.Role on ur.RoleId equals r.Id
                             //where r.Name == "Student"
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
            #endregion
            #region List Lesson
            var lessons = (from lp in db.LessonPackages
                           join l in db.Languages on lp.Languages_Id equals l.Id
                           join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                           where lp.Active == true
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
                    item.Id,
                    Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + string.Format("{0:N0}", item.Price) + ")"
                });
            }
            #endregion
            #region List Product
            var list_product = (from pr in db.Products
                                join pq in db.Products_Qty on pr.Id equals pq.Products_Id
                                where pq.Branches_Id == user_login.Branches_Id
                                select new { pr, pq }).ToList();
            List<object> products = new List<object>();
            foreach (var product in list_product)
            {
                products.Add(new
                {
                    product.pr.Id,
                    Name = product.pr.Description
                });
            }
            #endregion
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listVoucher = new SelectList(voucher_list, "Id", "Name");
            ViewBag.listCustomer = new SelectList(customer_list, "Id", "Name");
            ViewBag.listLesson = new SelectList(lesson_list, "Id", "Name");
            ViewBag.listProduct = new SelectList(products, "Id", "Name");

            return View(saleInvoicesViewModels);
        }

        private void SyncSaleInvoice_Inventory(Guid branch_id, Guid product_id, int qty, Guid sale_inv_items_id)
        {
            iSpeakContext context = new iSpeakContext();
            var inventory = context.Inventory.AsNoTracking().Where(x => x.Branches_Id == branch_id && x.Products_Id == product_id && x.AvailableQty > 0).OrderBy(x => x.ReceiveDate).FirstOrDefault();

            InventoryModels inventoryModels = context.Inventory.Find(inventory.Id);
            if (qty >= inventory.AvailableQty)
            {
                inventoryModels.AvailableQty = 0;
            }
            else
            {
                inventoryModels.AvailableQty -= qty;
            }
            context.Entry(inventoryModels).State = EntityState.Modified;

            SaleInvoiceItems_InventoryModels sii_i = new SaleInvoiceItems_InventoryModels
            {
                Id = Guid.NewGuid(),
                SaleInvoiceItems_Id = sale_inv_items_id,
                Inventory_Id = inventory.Id,
                Qty = inventory.AvailableQty - inventoryModels.AvailableQty
            };
            context.SaleInvoiceItems_Inventory.Add(sii_i);

            context.SaveChanges();

            if (qty > inventory.AvailableQty)
            {
                SyncSaleInvoice_Inventory(branch_id, product_id, qty - inventory.AvailableQty, sale_inv_items_id);
            }
        }
    }
}