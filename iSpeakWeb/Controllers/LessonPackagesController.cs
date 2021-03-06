﻿using iSpeak.Common;
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
        private readonly iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
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
                                SessionHours = lp.SessionHours,
                                Price = lp.Price,
                                Active = lp.Active
                            }).ToListAsync();
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(await data);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Languages_Id,LessonTypes_Id,SessionHours,ExpirationDay,Price,Notes,Active")] LessonPackagesModels lessonPackagesModels)
        {
            var check = db.LessonPackages.AsNoTracking()
                .Where(x => x.Name == lessonPackagesModels.Name
                            && x.Languages_Id == lessonPackagesModels.Languages_Id
                            && x.LessonTypes_Id == lessonPackagesModels.LessonTypes_Id
                            && x.SessionHours == lessonPackagesModels.SessionHours).ToList();
            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Lesson Package already existed.");
            }

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
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Languages_Id,LessonTypes_Id,SessionHours,ExpirationDay,Price,Notes,Active")] LessonPackagesModels lessonPackagesModels)
        {
            var check = db.LessonPackages.AsNoTracking()
                .Where(x => x.Id != lessonPackagesModels.Id 
                            && x.Name == lessonPackagesModels.Name 
                            && x.Languages_Id == lessonPackagesModels.Languages_Id 
                            && x.LessonTypes_Id == lessonPackagesModels.LessonTypes_Id 
                            && x.SessionHours == lessonPackagesModels.SessionHours).ToList();
            if (check.Count > 0)
            {
                ModelState.AddModelError("Duplicate", "This Lesson Package already existed.");
            }

            if (ModelState.IsValid)
            {
                var current_data = await db.LessonPackages.FindAsync(lessonPackagesModels.Id);
                current_data.Name = lessonPackagesModels.Name;
                current_data.Languages_Id = lessonPackagesModels.Languages_Id;
                current_data.LessonTypes_Id = lessonPackagesModels.LessonTypes_Id;
                current_data.SessionHours = lessonPackagesModels.SessionHours;
                current_data.ExpirationDay = lessonPackagesModels.ExpirationDay;
                current_data.Price = lessonPackagesModels.Price;
                current_data.Notes = lessonPackagesModels.Notes;
                current_data.Active = lessonPackagesModels.Active;
                db.Entry(current_data).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listTypes = new SelectList(db.LessonTypes.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(lessonPackagesModels);
        }
    }
}