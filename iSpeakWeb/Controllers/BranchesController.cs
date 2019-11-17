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
        private readonly iSpeakContext db = new iSpeakContext();
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

            Guid branch_id_before = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
            string status;
            var isValid = (from u in db.User
                           join ur in db.UserRole on u.Id equals ur.UserId
                           join a in db.Access on ur.RoleId equals a.RoleId
                           where u.UserName == User.Identity.Name && a.WebMenuAccess == "branches_change"
                           select u);
            if (isValid.Count() > 0)
            {
                status = "200";
                var user = await UserManager.FindByIdAsync(db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id);
                user.Branches_Id = branch_id;
                await UserManager.UpdateAsync(user); //update Branches_Id user
                await SignInManager.SignInAsync(user, false, false); //re-sign in to refresh cookie/claims in identity
            }
            else { status = "401"; }

            return Json(new { status, branch_id_before }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
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
                var current_data = await db.Branches.FindAsync(branchesModels.Id);
                current_data.Name = branchesModels.Name;
                current_data.Address = branchesModels.Address;
                current_data.PhoneNumber = branchesModels.PhoneNumber;
                current_data.Notes = branchesModels.Notes;
                current_data.InvoiceHeaderText = branchesModels.InvoiceHeaderText;
                current_data.Active = branchesModels.Active;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(branchesModels);
        }

        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        return View(await db.Branches.Where(x => x.Id == id).FirstOrDefaultAsync());
        //    }
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    BranchesModels branchesModels = await db.Branches.FindAsync(id);
        //    db.Branches.Remove(branchesModels);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}