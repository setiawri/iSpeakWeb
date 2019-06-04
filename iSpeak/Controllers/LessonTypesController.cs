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
    public class LessonTypesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.LessonTypes.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Active")] LessonTypesModels lessonTypesModels)
        {
            if (ModelState.IsValid)
            {
                lessonTypesModels.Id = Guid.NewGuid();
                lessonTypesModels.Active = true;
                db.LessonTypes.Add(lessonTypesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(lessonTypesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonTypesModels lessonTypesModels = await db.LessonTypes.FindAsync(id);
            if (lessonTypesModels == null)
            {
                return HttpNotFound();
            }
            return View(lessonTypesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Active")] LessonTypesModels lessonTypesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lessonTypesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(lessonTypesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.LessonTypes.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            LessonTypesModels lessonTypesModels = await db.LessonTypes.FindAsync(id);
            db.LessonTypes.Remove(lessonTypesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}