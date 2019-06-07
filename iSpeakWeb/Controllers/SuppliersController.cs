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
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Suppliers.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes,Active")] SuppliersModels suppliersModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(suppliersModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(suppliersModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Suppliers.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            SuppliersModels suppliersModels = await db.Suppliers.FindAsync(id);
            db.Suppliers.Remove(suppliersModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}