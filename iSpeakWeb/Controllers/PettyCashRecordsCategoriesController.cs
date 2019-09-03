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
    public class PettyCashRecordsCategoriesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(await db.PettyCashRecordsCategories.ToListAsync());
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Notes")] PettyCashRecordsCategoriesModels pettyCashRecordsCategoriesModels)
        {
            var check = db.PettyCashRecordsCategories.AsNoTracking().Where(x =>
                x.Name == pettyCashRecordsCategoriesModels.Name).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Petty Cash Category already existed.");
            }

            if (ModelState.IsValid)
            {
                pettyCashRecordsCategoriesModels.Id = Guid.NewGuid();
                pettyCashRecordsCategoriesModels.Default_row = false;
                pettyCashRecordsCategoriesModels.Active = true;
                db.PettyCashRecordsCategories.Add(pettyCashRecordsCategoriesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(pettyCashRecordsCategoriesModels);
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
                PettyCashRecordsCategoriesModels pettyCashRecordsCategoriesModels = await db.PettyCashRecordsCategories.FindAsync(id);
                if (pettyCashRecordsCategoriesModels == null)
                {
                    return HttpNotFound();
                }
                return View(pettyCashRecordsCategoriesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes,Default_row,Active")] PettyCashRecordsCategoriesModels pettyCashRecordsCategoriesModels)
        {
            var check = db.PettyCashRecordsCategories.AsNoTracking().Where(x =>
                x.Id != pettyCashRecordsCategoriesModels.Id
                && x.Name == pettyCashRecordsCategoriesModels.Name).ToList();

            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Petty Cash Category already existed.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(pettyCashRecordsCategoriesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View(pettyCashRecordsCategoriesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(await db.PettyCashRecordsCategories.Where(x => x.Id == id).FirstOrDefaultAsync());
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            PettyCashRecordsCategoriesModels pettyCashRecordsCategoriesModels = await db.PettyCashRecordsCategories.FindAsync(id);
            db.PettyCashRecordsCategories.Remove(pettyCashRecordsCategoriesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}