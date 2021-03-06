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
    public class ExpensesController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
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
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(await list);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }
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
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Timestamp,ExpenseCategories_Id,Description,Amount,Notes")] ExpensesModels expensesModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.Expenses.FindAsync(expensesModels.Id);
                current_data.Branches_Id = expensesModels.Branches_Id;
                current_data.Timestamp = expensesModels.Timestamp;
                current_data.ExpenseCategories_Id = expensesModels.ExpenseCategories_Id;
                current_data.Description = expensesModels.Description;
                current_data.Amount = expensesModels.Amount;
                current_data.Notes = expensesModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.ExpenseCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(expensesModels);
        }
    }
}