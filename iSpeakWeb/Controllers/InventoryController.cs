﻿using iSpeak.Common;
using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region Get Info
        public async Task<JsonResult> GetInfo(Guid id, int init_qty)
        {
            var list = await (from si in db.SaleInvoices
                              join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                              join siii in db.SaleInvoiceItems_Inventory on sii.Id equals siii.SaleInvoiceItems_Id
                              join i in db.Inventory on siii.Inventory_Id equals i.Id
                              where si.Cancelled == false && i.Id == id
                              orderby si.Timestamp
                              select new { si, sii, siii, i }).ToListAsync();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Sale Invoice</th>
                                                <th>Qty</th>
                                                <th>Balance</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                init_qty -= item.siii.Qty;
                message += @"<tr>
                                <td>" + item.si.No + @"</td>
                                <td>" + item.siii.Qty + @"</td>
                                <td>" + init_qty + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
                var data = await (from i in db.Inventory
                                  join b in db.Branches on i.Branches_Id equals b.Id
                                  join pr in db.Products on i.Products_Id equals pr.Id
                                  join u in db.Units on pr.Units_Id equals u.Id
                                  join s in db.Suppliers on i.Suppliers_Id equals s.Id
                                  where i.Branches_Id == user_login.Branches_Id
                                  select new { i, b, pr, u, s }).ToListAsync();

                List<InventoryViewModels> list = new List<InventoryViewModels>();
                foreach (var item in data)
                {
                    list.Add(new InventoryViewModels
                    {
                        Id = item.i.Id,
                        Branch = item.b.Name,
                        Product = item.pr.Description,
                        Unit = item.u.Name,
                        ReceiveDate = TimeZoneInfo.ConvertTimeFromUtc(item.i.ReceiveDate, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                        BuyQty = item.i.BuyQty,
                        AvailableQty = item.i.AvailableQty,
                        Supplier = item.s.Name,
                        BuyPrice = item.i.BuyPrice
                    });
                }
                ViewBag.BuyPrice = p.IsGranted(User.Identity.Name, "show_buyprice");
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
                ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,Products_Id,ReceiveDate,BuyQty,AvailableQty,Suppliers_Id,BuyPrice,Notes")] InventoryModels inventoryModels)
        {
            if (ModelState.IsValid)
            {
                inventoryModels.Id = Guid.NewGuid();
                inventoryModels.ReceiveDate = new DateTime(inventoryModels.ReceiveDate.Year, inventoryModels.ReceiveDate.Month, inventoryModels.ReceiveDate.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                inventoryModels.AvailableQty = inventoryModels.BuyQty;
                db.Inventory.Add(inventoryModels);

                Products_QtyModels pq = await db.Products_Qty.Where(x => x.Branches_Id == inventoryModels.Branches_Id && x.Products_Id == inventoryModels.Products_Id).FirstOrDefaultAsync();
                if (pq == null)
                {
                    Products_QtyModels products_QtyModels = new Products_QtyModels
                    {
                        Id = Guid.NewGuid(),
                        Branches_Id = inventoryModels.Branches_Id,
                        Products_Id = inventoryModels.Products_Id,
                        Qty = inventoryModels.BuyQty
                    };
                    db.Products_Qty.Add(products_QtyModels);
                }
                else
                {
                    pq.Qty += inventoryModels.BuyQty;
                    db.Entry(pq).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
            ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(inventoryModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                InventoryModels inventoryModels = await db.Inventory.FindAsync(id);
                if (inventoryModels == null)
                {
                    return HttpNotFound();
                }
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
                ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View(inventoryModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Products_Id,ReceiveDate,BuyQty,AvailableQty,Suppliers_Id,BuyPrice,Notes")] InventoryModels inventoryModels)
        {
            if (ModelState.IsValid)
            {
                //inventoryModels.ReceiveDate = new DateTime(inventoryModels.ReceiveDate.Year, inventoryModels.ReceiveDate.Month, inventoryModels.ReceiveDate.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                //inventoryModels.AvailableQty = inventoryModels.BuyQty;
                //db.Entry(inventoryModels).State = EntityState.Modified;

                #region Substract Qty
                InventoryModels inventoryModels_before = await db.Inventory.AsNoTracking().Where(x => x.Id == inventoryModels.Id).FirstOrDefaultAsync();
                Products_QtyModels pq_substract = await db.Products_Qty.Where(x => x.Branches_Id == inventoryModels_before.Branches_Id && x.Products_Id == inventoryModels_before.Products_Id).FirstOrDefaultAsync();
                pq_substract.Qty -= inventoryModels_before.BuyQty;
                db.Entry(pq_substract).State = EntityState.Modified;
                #endregion
                #region Added Qty
                Products_QtyModels pq_add = await db.Products_Qty.Where(x => x.Branches_Id == inventoryModels.Branches_Id && x.Products_Id == inventoryModels.Products_Id).FirstOrDefaultAsync();
                pq_add.Qty += inventoryModels.BuyQty;
                db.Entry(pq_add).State = EntityState.Modified;
                #endregion
                #region Inventory Update
                var current_data = await db.Inventory.FindAsync(inventoryModels.Id);
                current_data.ReceiveDate = new DateTime(inventoryModels.ReceiveDate.Year, inventoryModels.ReceiveDate.Month, inventoryModels.ReceiveDate.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                current_data.Products_Id = inventoryModels.Products_Id;
                current_data.Suppliers_Id = inventoryModels.Suppliers_Id;
                current_data.BuyQty = inventoryModels.BuyQty;
                current_data.AvailableQty = (inventoryModels.BuyQty - inventoryModels_before.BuyQty) + inventoryModels.AvailableQty;
                current_data.BuyPrice = inventoryModels.BuyPrice;
                current_data.Notes = inventoryModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                #endregion

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
            ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(inventoryModels);
        }

        public async Task<ActionResult> Stock()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
                var list = await (from pq in db.Products_Qty
                                  join b in db.Branches on pq.Branches_Id equals b.Id
                                  join pr in db.Products on pq.Products_Id equals pr.Id
                                  join u in db.Units on pr.Units_Id equals u.Id
                                  where pq.Branches_Id == user_login.Branches_Id
                                  select new StockOnHandViewModels
                                  {
                                      Product = pr.Description,
                                      Qty = pq.Qty,
                                      Unit = u.Name,
                                      ForSale = pr.ForSale
                                  }).ToListAsync();
                return View(list);
            }
        }

        public async Task<ActionResult> Excel()
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            string branch = db.Branches.Where(x => x.Id == user_login.Branches_Id).FirstOrDefault().Name;
            List<StockOnHandViewModels> list = await (from pq in db.Products_Qty
                                                      join b in db.Branches on pq.Branches_Id equals b.Id
                                                      join pr in db.Products on pq.Products_Id equals pr.Id
                                                      join u in db.Units on pr.Units_Id equals u.Id
                                                      where pq.Branches_Id == user_login.Branches_Id
                                                      select new StockOnHandViewModels
                                                      {
                                                          Product = pr.Description,
                                                          Qty = pq.Qty,
                                                          Unit = u.Name,
                                                          ForSale = pr.ForSale
                                                      }).ToListAsync();

            ExportExcel ee = new ExportExcel();
            var fileDownloadName = "InventoryStock" + ".xls";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var package = ee.StockOnHand(branch, list);

            var fileStream = new MemoryStream();
            package.SaveAs(fileStream);
            fileStream.Position = 0;

            var fsr = new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = fileDownloadName
            };

            return fsr;
        }

        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        var data = (from i in db.Inventory
        //                    join pr in db.Products on i.Products_Id equals pr.Id
        //                    join u in db.Units on pr.Units_Id equals u.Id
        //                    join s in db.Suppliers on i.Suppliers_Id equals s.Id
        //                    where i.Id == id
        //                    select new InventoryViewModels
        //                    {
        //                        Id = i.Id,
        //                        Product = pr.Description,
        //                        Unit = u.Name,
        //                        ReceiveDate = i.ReceiveDate,
        //                        BuyQty = i.BuyQty,
        //                        AvailableQty = i.AvailableQty,
        //                        Supplier = s.Name,
        //                        BuyPrice = i.BuyPrice
        //                    }).FirstOrDefaultAsync();
        //        return View(await data);
        //    }
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    InventoryModels inventoryModels = await db.Inventory.FindAsync(id);
        //    db.Inventory.Remove(inventoryModels);

        //    Products_QtyModels products_QtyModels = await db.Products_Qty.Where(x => x.Branches_Id == inventoryModels.Branches_Id && x.Products_Id == inventoryModels.Products_Id).FirstOrDefaultAsync();
        //    products_QtyModels.Qty -= inventoryModels.BuyQty;
        //    db.Entry(products_QtyModels).State = EntityState.Modified;
            
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}