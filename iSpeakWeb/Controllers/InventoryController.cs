using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
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

            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
            ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Products_Id,ReceiveDate,BuyQty,AvailableQty,Suppliers_Id,BuyPrice,Notes")] InventoryModels inventoryModels)
        {
            if (ModelState.IsValid)
            {
                inventoryModels.ReceiveDate = new DateTime(inventoryModels.ReceiveDate.Year, inventoryModels.ReceiveDate.Month, inventoryModels.ReceiveDate.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                inventoryModels.AvailableQty = inventoryModels.BuyQty;
                db.Entry(inventoryModels).State = EntityState.Modified;

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

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listProduct = new SelectList(db.Products.Where(x => x.ForSale == true && x.Active == true).OrderBy(x => x.Description).ToList(), "Id", "Description");
            ViewBag.listSupplier = new SelectList(db.Suppliers.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(inventoryModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from i in db.Inventory
                        join pr in db.Products on i.Products_Id equals pr.Id
                        join u in db.Units on pr.Units_Id equals u.Id
                        join s in db.Suppliers on i.Suppliers_Id equals s.Id
                        where i.Id == id
                        select new InventoryViewModels
                        {
                            Id = i.Id,
                            Product = pr.Description,
                            Unit = u.Name,
                            ReceiveDate = i.ReceiveDate,
                            BuyQty = i.BuyQty,
                            AvailableQty = i.AvailableQty,
                            Supplier = s.Name,
                            BuyPrice = i.BuyPrice
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            InventoryModels inventoryModels = await db.Inventory.FindAsync(id);
            db.Inventory.Remove(inventoryModels);

            Products_QtyModels products_QtyModels = await db.Products_Qty.Where(x => x.Branches_Id == inventoryModels.Branches_Id && x.Products_Id == inventoryModels.Products_Id).FirstOrDefaultAsync();
            products_QtyModels.Qty -= inventoryModels.BuyQty;
            db.Entry(products_QtyModels).State = EntityState.Modified;
            
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}