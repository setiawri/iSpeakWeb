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
    public class ConsignmentsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Consignments.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Branches_Id,Notes,Active")] ConsignmentsModels consignmentsModels)
        {
            if (ModelState.IsValid)
            {
                consignmentsModels.Id = Guid.NewGuid();
                consignmentsModels.Active = true;
                db.Consignments.Add(consignmentsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(consignmentsModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConsignmentsModels consignmentsModels = await db.Consignments.FindAsync(id);
            if (consignmentsModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(consignmentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Branches_Id,Notes,Active")] ConsignmentsModels consignmentsModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consignmentsModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(consignmentsModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Consignments.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ConsignmentsModels consignmentsModels = await db.Consignments.FindAsync(id);
            db.Consignments.Remove(consignmentsModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}