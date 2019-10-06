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
    public class ExpenseCategoriesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.ExpenseCategories.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Notes,Active")] ExpenseCategoriesModels expenseCategoriesModels)
        {
            if (ModelState.IsValid)
            {
                expenseCategoriesModels.Id = Guid.NewGuid();
                expenseCategoriesModels.Active = true;
                db.ExpenseCategories.Add(expenseCategoriesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(expenseCategoriesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseCategoriesModels expenseCategoriesModels = await db.ExpenseCategories.FindAsync(id);
            if (expenseCategoriesModels == null)
            {
                return HttpNotFound();
            }
            return View(expenseCategoriesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes,Active")] ExpenseCategoriesModels expenseCategoriesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(expenseCategoriesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(expenseCategoriesModels);
        }
    }
}