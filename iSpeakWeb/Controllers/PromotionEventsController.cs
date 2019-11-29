using iSpeak.Common;
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
    public class PromotionEventsController : Controller
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
                ViewBag.Add = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_create");
                ViewBag.Edit = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_edit");
                return View(await db.PromotionEvents.ToListAsync());
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
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,Name,Location,TotalDays,EventFee,PersonnelCost,AdditionalCost,Notes")] PromotionEventsModels promotionEventsModels)
        {
            if (ModelState.IsValid)
            {
                promotionEventsModels.Id = Guid.NewGuid();
                db.PromotionEvents.Add(promotionEventsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(promotionEventsModels);
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
                PromotionEventsModels promotionEventsModels = await db.PromotionEvents.FindAsync(id);
                if (promotionEventsModels == null)
                {
                    return HttpNotFound();
                }
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View(promotionEventsModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Name,Location,TotalDays,EventFee,PersonnelCost,AdditionalCost,Notes")] PromotionEventsModels promotionEventsModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.PromotionEvents.FindAsync(promotionEventsModels.Id);
                current_data.Branches_Id = promotionEventsModels.Branches_Id;
                current_data.Name = promotionEventsModels.Name;
                current_data.Location = promotionEventsModels.Location;
                current_data.TotalDays = promotionEventsModels.TotalDays;
                current_data.EventFee = promotionEventsModels.EventFee;
                current_data.PersonnelCost = promotionEventsModels.PersonnelCost;
                current_data.AdditionalCost = promotionEventsModels.AdditionalCost;
                current_data.Notes = promotionEventsModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(promotionEventsModels);
        }
    }
}