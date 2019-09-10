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
    public class ServicesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var list = (from s in db.Services
                            join u in db.Units on s.Units_Id equals u.Id
                            select new ServicesViewModels
                            {
                                Id = s.Id,
                                Description = s.Description,
                                ForSale = s.ForSale,
                                Unit = u.Name,
                                SellPrice = s.SellPrice,
                                Active = s.Active
                            }).ToListAsync();
                return View(await list);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Units_Id,ForSale,SellPrice,Notes")] ServicesModels servicesModels)
        {
            var check = db.Services.AsNoTracking().Where(x =>
                x.Description == servicesModels.Description
                && x.Units_Id == servicesModels.Units_Id).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Service already existed.");
            }

            if (ModelState.IsValid)
            {
                servicesModels.Id = Guid.NewGuid();
                servicesModels.Active = true;
                db.Services.Add(servicesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View(servicesModels);
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
                ServicesModels servicesModels = await db.Services.FindAsync(id);
                if (servicesModels == null)
                {
                    return HttpNotFound();
                }
                ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
                return View(servicesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Units_Id,ForSale,SellPrice,Notes,Active")] ServicesModels servicesModels)
        {
            var check = db.Services.AsNoTracking().Where(x =>
                x.Id != servicesModels.Id
                && x.Description == servicesModels.Description
                && x.Units_Id == servicesModels.Units_Id).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Service already existed.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(servicesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listUnit = new SelectList(db.Units.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
            return View(servicesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(await db.Services.Where(x => x.Id == id).FirstOrDefaultAsync());
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ServicesModels servicesModels = await db.Services.FindAsync(id);
            db.Services.Remove(servicesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}