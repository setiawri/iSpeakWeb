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
    public class ExpenseCategoriesController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(await db.ExpenseCategories.OrderBy(x => x.Name).ToListAsync());
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View();
            }
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
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes,Active")] ExpenseCategoriesModels expenseCategoriesModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.ExpenseCategories.FindAsync(expenseCategoriesModels.Id);
                current_data.Name = expenseCategoriesModels.Name;
                current_data.Notes = expenseCategoriesModels.Notes;
                current_data.Active = expenseCategoriesModels.Active;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(expenseCategoriesModels);
        }
    }
}