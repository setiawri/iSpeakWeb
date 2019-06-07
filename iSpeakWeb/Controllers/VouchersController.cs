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
    public class VouchersController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Vouchers.OrderBy(x => x.Code).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Code,Description,Amount,Notes,Active")] VouchersModels vouchersModels)
        {
            if (ModelState.IsValid)
            {
                vouchersModels.Id = Guid.NewGuid();
                vouchersModels.Active = true;
                db.Vouchers.Add(vouchersModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(vouchersModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VouchersModels vouchersModels = await db.Vouchers.FindAsync(id);
            if (vouchersModels == null)
            {
                return HttpNotFound();
            }
            return View(vouchersModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Code,Description,Amount,Notes,Active")] VouchersModels vouchersModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vouchersModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(vouchersModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Vouchers.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            VouchersModels vouchersModels = await db.Vouchers.FindAsync(id);
            db.Vouchers.Remove(vouchersModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}