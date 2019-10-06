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
    public class ExpensesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
            var list = (from e in db.Expenses
                        join ec in db.ExpenseCategories on e.ExpenseCategories_Id equals ec.Id
                        where e.Branches_Id == user_branch
                        select new ExpensesViewModels
                        {
                            Id = e.Id,
                            Date = e.Timestamp,
                            Category = ec.Name,
                            Description = e.Description,
                            Amount = e.Amount,
                            Notes = e.Notes
                        }).ToListAsync();
            return View(await list);
        }

        public ActionResult Create()
        {
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,Timestamp,ExpenseCategories_Id,Description,Amount,Notes")] ExpensesModels expensesModels)
        {
            if (ModelState.IsValid)
            {
                expensesModels.Id = Guid.NewGuid();
                db.Expenses.Add(expensesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(expensesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpensesModels expensesModels = await db.Expenses.FindAsync(id);
            if (expensesModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(expensesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Timestamp,ExpenseCategories_Id,Description,Amount,Notes")] ExpensesModels expensesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(expensesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(expensesModels);
        }
    }
}