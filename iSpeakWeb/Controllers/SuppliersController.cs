using iSpeak.Common;
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
    public class SuppliersController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(await db.Suppliers.OrderBy(x => x.Name).ToListAsync());
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Notes,Active")] SuppliersModels suppliersModels)
        {
            if (ModelState.IsValid)
            {
                suppliersModels.Id = Guid.NewGuid();
                suppliersModels.Active = true;
                db.Suppliers.Add(suppliersModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(suppliersModels);
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
                SuppliersModels suppliersModels = await db.Suppliers.FindAsync(id);
                if (suppliersModels == null)
                {
                    return HttpNotFound();
                }
                return View(suppliersModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes,Active")] SuppliersModels suppliersModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.Suppliers.FindAsync(suppliersModels.Id);
                current_data.Name = suppliersModels.Name;
                current_data.Notes = suppliersModels.Notes;
                current_data.Active = suppliersModels.Active;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(suppliersModels);
        }

        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        return View(await db.Suppliers.Where(x => x.Id == id).FirstOrDefaultAsync());
        //    }
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    SuppliersModels suppliersModels = await db.Suppliers.FindAsync(id);
        //    db.Suppliers.Remove(suppliersModels);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}