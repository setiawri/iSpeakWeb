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
            var user = (from u in db.User
                        join ur in db.UserRole on u.Id equals ur.UserId
                        join r in db.Role on ur.RoleId equals r.Id
                        orderby u.Firstname ascending
                        select new UserViewModels
                        {
                            Id = u.Id,
                            Fullname = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            UserName = u.UserName,
                            Email = u.Email,
                            Role = r.Name,
                            RoleId = r.Id,
                            Birthday = u.Birthday,
                            Active = u.Active
                        });

            return View(user.ToList());
        }

        public ActionResult Edit(string id)
        {
            var user = (from u in db.User
                        join ur in db.UserRole on u.Id equals ur.UserId
                        join r in db.Role on ur.RoleId equals r.Id
                        where u.Id == id
                        select new UserViewModels
                        {
                            Id = u.Id,
                            Fullname = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Firstname = u.Firstname,
                            Middlename = u.Middlename,
                            Lastname = u.Lastname,
                            UserName = u.UserName,
                            Address = u.Address,
                            Email = u.Email,
                            Phone1 = u.Phone1,
                            Phone2 = u.Phone2,
                            Role = r.Name,
                            RoleId = r.Id,
                            Birthday = u.Birthday,
                            Notes = u.Notes,
                            Active = u.Active
                        }).FirstOrDefault();
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Role,RoleId,UserName,Fullname,Firstname,Middlename,Lastname,Address,Phone1,Phone2,Email,Birthday,Notes,Active")] UserViewModels userViewModels)
        {
            if (ModelState.IsValid)
            {
                var user = new UserModels()
                {
                    Id = userViewModels.Id,
                    Email = userViewModels.Email,
                    Phone1 = userViewModels.Phone1,
                    Phone2 = userViewModels.Phone2,
                    Address = userViewModels.Address,
                    Notes = userViewModels.Notes,
                    Active = userViewModels.Active
                };
                var userRole = new UserRoleModels() { UserId = userViewModels.Id, RoleId = userViewModels.RoleId };

                using (var database = new iSpeakContext())
                {
                    database.User.Attach(user);
                    database.Entry(user).Property(x => x.Email).IsModified = true;
                    database.Entry(user).Property(x => x.Phone1).IsModified = true;
                    database.Entry(user).Property(x => x.Phone2).IsModified = true;
                    database.Entry(user).Property(x => x.Address).IsModified = true;
                    database.Entry(user).Property(x => x.Notes).IsModified = true;
                    database.Entry(user).Property(x => x.Active).IsModified = true;

                    database.UserRole.Attach(userRole);
                    database.Entry(userRole).Property(x => x.RoleId).IsModified = true;

                    database.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(userViewModels);
        }
    }
}