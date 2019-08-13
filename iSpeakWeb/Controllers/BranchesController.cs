using iSpeak.Common;
using iSpeak.Models;
using Microsoft.AspNet.Identity.Owin;
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
    public class BranchesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public BranchesController()
        {
        }

        public BranchesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public async Task<JsonResult> ChangeBranch(Guid branch_id)
        {
            //var session_login = Session["Login"] as LoginViewModel;
            //session_login.Branches_Id = branch_id;
            
            var user = await UserManager.FindByIdAsync(db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id);
            user.Branches_Id = branch_id;
            await UserManager.UpdateAsync(user); //update Branches_Id user
            await SignInManager.SignInAsync(user, false, false); //re-sign in to refresh cookie/claims in identity

            string status = "200";

            return Json(new { status }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(await db.Branches.OrderBy(x => x.Name).ToListAsync());
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Address,PhoneNumber,Notes,InvoiceHeaderText,Active")] BranchesModels branchesModels)
        {
            if (ModelState.IsValid)
            {
                branchesModels.Id = Guid.NewGuid();
                branchesModels.Active = true;
                db.Branches.Add(branchesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(branchesModels);
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
                BranchesModels branchesModels = await db.Branches.FindAsync(id);
                if (branchesModels == null)
                {
                    return HttpNotFound();
                }
                return View(branchesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Address,PhoneNumber,Notes,InvoiceHeaderText,Active")] BranchesModels branchesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(branchesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(branchesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(await db.Branches.Where(x => x.Id == id).FirstOrDefaultAsync());
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            BranchesModels branchesModels = await db.Branches.FindAsync(id);
            db.Branches.Remove(branchesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}