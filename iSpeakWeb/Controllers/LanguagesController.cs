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
    public class LanguagesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Languages.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Active")] LanguagesModels languagesModels)
        {
            if (ModelState.IsValid)
            {
                languagesModels.Id = Guid.NewGuid();
                languagesModels.Active = true;
                db.Languages.Add(languagesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(languagesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LanguagesModels languagesModels = await db.Languages.FindAsync(id);
            if (languagesModels == null)
            {
                return HttpNotFound();
            }
            return View(languagesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Active")] LanguagesModels languagesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(languagesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(languagesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Languages.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            LanguagesModels languagesModels = await db.Languages.FindAsync(id);
            db.Languages.Remove(languagesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}