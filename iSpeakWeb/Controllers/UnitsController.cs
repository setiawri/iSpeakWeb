﻿using iSpeak.Models;
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
    public class UnitsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Units.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Active")] UnitsModels unitsModels)
        {
            if (ModelState.IsValid)
            {
                unitsModels.Id = Guid.NewGuid();
                unitsModels.Active = true;
                db.Units.Add(unitsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(unitsModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitsModels unitsModels = await db.Units.FindAsync(id);
            if (unitsModels == null)
            {
                return HttpNotFound();
            }
            return View(unitsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Active")] UnitsModels unitsModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(unitsModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(unitsModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Units.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            UnitsModels unitsModels = await db.Units.FindAsync(id);
            db.Units.Remove(unitsModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}