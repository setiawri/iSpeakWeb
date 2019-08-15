using iSpeak.Common;
using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        // GET: User
        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View(db.User.ToList());
            }
        }

        public ActionResult Edit(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                EditUserViewModels editUserViewModels = new EditUserViewModels();
                editUserViewModels.User = db.User.Where(x => x.Id == id).FirstOrDefault();
                editUserViewModels.RoleId = db.UserRole.Where(x => x.UserId == id).Select(x => x.RoleId).ToList();
                ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View(editUserViewModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "User,RoleId")] EditUserViewModels editUserViewModels)
        {
            if (ModelState.IsValid)
            {
                var user = new UserModels()
                {
                    Id = editUserViewModels.User.Id,
                    Firstname = editUserViewModels.User.Firstname,
                    Middlename = editUserViewModels.User.Middlename,
                    Lastname = editUserViewModels.User.Lastname,
                    Birthday = editUserViewModels.User.Birthday,
                    UserName = editUserViewModels.User.UserName,
                    Email = editUserViewModels.User.Email,
                    Phone1 = editUserViewModels.User.Phone1,
                    Phone2 = editUserViewModels.User.Phone2,
                    Address = editUserViewModels.User.Address,
                    Notes = editUserViewModels.User.Notes,
                    Active = editUserViewModels.User.Active,
                    Branches_Id = editUserViewModels.User.Branches_Id
                };
                //var userRole = new UserRoleModels() { UserId = userViewModels.Id, RoleId = userViewModels.RoleId };

                using (var database = new iSpeakContext())
                {
                    database.User.Attach(user);
                    database.Entry(user).Property(x => x.Firstname).IsModified = true;
                    database.Entry(user).Property(x => x.Middlename).IsModified = true;
                    database.Entry(user).Property(x => x.Lastname).IsModified = true;
                    database.Entry(user).Property(x => x.Birthday).IsModified = true;
                    database.Entry(user).Property(x => x.UserName).IsModified = true;
                    database.Entry(user).Property(x => x.Email).IsModified = true;
                    database.Entry(user).Property(x => x.Phone1).IsModified = true;
                    database.Entry(user).Property(x => x.Phone2).IsModified = true;
                    database.Entry(user).Property(x => x.Address).IsModified = true;
                    database.Entry(user).Property(x => x.Notes).IsModified = true;
                    database.Entry(user).Property(x => x.Active).IsModified = true;
                    database.Entry(user).Property(x => x.Branches_Id).IsModified = true;

                    int deleteUserRole = database.Database.ExecuteSqlCommand("DELETE FROM AspNetUserRoles WHERE UserId='" + editUserViewModels.User.Id + "'");

                    //database.UserRole.Attach(userRole);
                    //database.Entry(userRole).Property(x => x.RoleId).IsModified = true;

                    database.SaveChanges();
                }

                //var list_role_before = db.UserRole.AsNoTracking().Where(x => x.UserId == editUserViewModels.User.Id).ToList();
                //foreach (var role in list_role_before)
                //{
                //    db.UserRole.Remove(role);
                //}

                foreach (var role_id in editUserViewModels.RoleId)
                {
                    //UserRoleModels urm = new UserRoleModels();
                    //urm.UserId = editUserViewModels.User.Id;
                    //urm.RoleId = role_id;
                    //db.UserRole.Add(urm);
                    using (var ctx = new iSpeakContext())
                    {
                        int updateUserRole = ctx.Database.ExecuteSqlCommand("INSERT INTO AspNetUserRoles VALUES ('" + editUserViewModels.User.Id + "','" + role_id + "')");
                    }
                }
                //db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(editUserViewModels);
        }
    }
}