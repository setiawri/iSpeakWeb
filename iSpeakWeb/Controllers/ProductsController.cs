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
    public class ProductsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        #region GetProductAvailable
        public async Task<JsonResult> GetProductAvailable(Guid product_id)
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            var product_qty = await db.Products_Qty.Where(x => x.Branches_Id == user_login.Branches_Id && x.Products_Id == product_id).FirstOrDefaultAsync();
            
            string color;
            if (product_qty.Qty > 2) { color = "badge-primary"; }
            else if (product_qty.Qty >= 1) { color = "badge-warning"; }
            else { color = "badge-danger"; }

            string badge = "<span class='badge " + color + " d-block'>" + string.Format("{0:N0}", product_qty.Qty) + " Available</span>";

            return Json(new { badge }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetStockOnHand
        public async Task<JsonResult> GetStockOnHand(Guid branched_id)
        {
            //var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            var list = await (from pq in db.Products_Qty
                              join b in db.Branches on pq.Branches_Id equals b.Id
                              join pr in db.Products on pq.Products_Id equals pr.Id
                              join u in db.Units on pr.Units_Id equals u.Id
                              where pq.Branches_Id == branched_id
                              select new { pq, b, pr, u }).ToListAsync();

            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Branch</th>
                                                <th>Product</th>
                                                <th>Qty</th>
                                                <th>Unit</th>
                                                <th>For Sale</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                string badge = (item.pr.ForSale)
                                ? "<span class='badge badge-success'>Yes</span>"
                                : "<span class='badge badge-danger'>No</span>";

                message += @"<tr>
                                <td>" + item.b.Name + @"</td>
                                <td>" + item.pr.Description + @"</td>
                                <td>" + string.Format("{0:N0}", item.pq.Qty) + @"</td>
                                <td>" + item.u.Name + @"</td>
                                <td>" + badge + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public async Task<ActionResult> Index()
        {
            var list = (from pr in db.Products
                        join u in db.Units on pr.Units_Id equals u.Id
                        select new ProductsViewModels
                        {
                            Id = pr.Id,
                            Description = pr.Description,
                            Barcode = pr.Barcode,
                            ForSale = pr.ForSale,
                            Unit = u.Name,
                            SellPrice = pr.SellPrice,
                            Active = pr.Active
                        }).ToListAsync();
            return View(await list);
        }

        public ActionResult Create()
        {
            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Units_Id,Barcode,ForSale,SellPrice,Notes")] ProductsModels productsModels)
        {
            var check = db.Products.AsNoTracking().Where(x => 
                x.Description == productsModels.Description 
                && x.Units_Id == productsModels.Units_Id).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Product already existed.");
            }

            if (ModelState.IsValid)
            {
                productsModels.Id = Guid.NewGuid();
                productsModels.Active = true;
                db.Products.Add(productsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View(productsModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductsModels productsModels = await db.Products.FindAsync(id);
            if (productsModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View(productsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Units_Id,Barcode,ForSale,SellPrice,Notes,Active")] ProductsModels productsModels)
        {
            var check = db.Products.AsNoTracking().Where(x =>
                x.Id != productsModels.Id
                && x.Description == productsModels.Description
                && x.Units_Id == productsModels.Units_Id).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Product already existed.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(productsModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View(productsModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Products.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ProductsModels productsModels = await db.Products.FindAsync(id);
            db.Products.Remove(productsModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}