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
    public class LessonPackagesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            var data = (from lp in db.LessonPackages
                        join l in db.Languages on lp.Languages_Id equals l.Id
                        join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                        select new LessonPackagesViewModels
                        {
                            Id = lp.Id,
                            Name = lp.Name,
                            Languages = l.Name,
                            LessonTypes = lt.Name,
                            Price = lp.Price,
                            Active = lp.Active
                        }).ToListAsync();
            return View(await data);
        }

        public ActionResult Create()
        {
            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Languages_Id,LessonTypes_Id,SessionHours,ExpirationDay,Price,Notes,Active")] LessonPackagesModels lessonPackagesModels)
        {
            if (ModelState.IsValid)
            {
                lessonPackagesModels.Id = Guid.NewGuid();
                lessonPackagesModels.Active = true;
                db.LessonPackages.Add(lessonPackagesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(lessonPackagesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LessonPackagesModels lessonPackagesModels = await db.LessonPackages.FindAsync(id);
            if (lessonPackagesModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(lessonPackagesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Languages_Id,LessonTypes_Id,SessionHours,ExpirationDay,Price,Notes,Active")] LessonPackagesModels lessonPackagesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lessonPackagesModels).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(lessonPackagesModels);
        }
    }
}