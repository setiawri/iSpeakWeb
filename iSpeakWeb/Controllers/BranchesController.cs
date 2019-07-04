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
    public class BranchesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Branches.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Address,PhoneNumber,Notes,Active")] BranchesModels branchesModels)
        {
            if (ModelState.IsValid)
            {
                branchesModels.Id = Guid.NewGuid();
                branchesModels.Active = true;
                db.Branches.Add(branchesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(branchesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BranchesModels branchesModels = await db.Branches.FindAsync(id);
            if (branchesModels == null)
            {
                return HttpNotFound();
            }
            return View(branchesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Address,PhoneNumber,Notes,Active")] BranchesModels branchesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(branchesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(branchesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Branches.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            BranchesModels branchesModels = await db.Branches.FindAsync(id);
            db.Branches.Remove(branchesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}