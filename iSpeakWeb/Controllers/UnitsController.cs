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
    public class UnitsController : Controller
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
                return View(await db.Units.OrderBy(x => x.Name).ToListAsync());
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Active")] UnitsModels unitsModels)
        {
            if (ModelState.IsValid)
            {
                unitsModels.Id = Guid.NewGuid();
                unitsModels.Active = true;
                db.Units.Add(unitsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(unitsModels);
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
                UnitsModels unitsModels = await db.Units.FindAsync(id);
                if (unitsModels == null)
                {
                    return HttpNotFound();
                }
                return View(unitsModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Active")] UnitsModels unitsModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.Units.FindAsync(unitsModels.Id);
                current_data.Name = unitsModels.Name;
                current_data.Active = unitsModels.Active;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(unitsModels);
        }

        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        return View(await db.Units.Where(x => x.Id == id).FirstOrDefaultAsync());
        //    }
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    UnitsModels unitsModels = await db.Units.FindAsync(id);
        //    db.Units.Remove(unitsModels);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}