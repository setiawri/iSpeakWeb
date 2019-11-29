using iSpeak.Common;
using iSpeak.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region GET ACTIVE USER
        public async Task<JsonResult> GetUser(string search, int page, int limit, string role)
        {
            int offset = limit * (page - 1);
            List<UserModels> list = new List<UserModels>();
            if (string.IsNullOrEmpty(search))
            {
                if (role.ToLower() == "all")
                {
                    list = await db.User.Where(x => x.Active == true).OrderBy(x => x.Firstname).Skip(offset).Take(limit).ToListAsync();
                }
                else
                {
                    var items = await (from u in db.User
                                       join ur in db.UserRole on u.Id equals ur.UserId
                                       join r in db.Role on ur.RoleId equals r.Id
                                       where u.Active == true && r.Name.ToLower() == role.ToLower()
                                       orderby u.Firstname
                                       select new { u, ur, r }).Skip(offset).Take(limit).ToListAsync();
                    foreach (var i in items)
                    {
                        list.Add(i.u);
                    }
                }
            }
            else
            {
                if (role.ToLower() == "all")
                {
                    list = await db.User.Where(x =>
                        x.Active == true && x.Firstname.Contains(search) || x.Middlename.Contains(search) || x.Lastname.Contains(search)
                    ).OrderBy(x => x.Firstname).Skip(offset).Take(limit).ToListAsync();
                }
                else
                {
                    var items = await (from u in db.User
                                       join ur in db.UserRole on u.Id equals ur.UserId
                                       join r in db.Role on ur.RoleId equals r.Id
                                       where u.Active == true && r.Name.ToLower() == role.ToLower() &&
                                         (u.Firstname.Contains(search) || u.Middlename.Contains(search) || u.Lastname.Contains(search))
                                       orderby u.Firstname
                                       select new { u, ur, r }).Skip(offset).Take(limit).ToListAsync();
                    foreach (var i in items)
                    {
                        list.Add(i.u);
                    }
                }
            }

            List<Select2Pagination.Select2Results> results = new List<Select2Pagination.Select2Results>();
            foreach (var item in list)
            {
                results.Add(new Select2Pagination.Select2Results
                {
                    id = item.Id,
                    text = item.Firstname + " " + item.Middlename + " " + item.Lastname
                });
            }

            Select2Pagination.Select2Page pagination = new Select2Pagination.Select2Page
            {
                more = results.Count() == limit ? true : false
            };

            return Json(new { results, pagination }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Logs
        public async Task<JsonResult> GetLogs(string id)
        {
            //var list = await db.Logs.Where(x => x.RefId.ToString() == id).OrderByDescending(x => x.Timestamp).ToListAsync();
            var list = await (from l in db.Logs
                              join u in db.User on l.UserAccounts_Id equals u.Id
                              where l.RefId.ToString() == id
                              orderby l.Timestamp descending
                              select new { l, u }).ToListAsync();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Timestamp</th>
                                                <th>User</th>
                                                <th>Description</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                string description = item.l.Action == "Modified"
                    ? string.Format("Updated {0}: {1} to {2}", item.l.ColumnName, string.IsNullOrEmpty(item.l.OriginalValue) ? "' '" : item.l.OriginalValue, string.IsNullOrEmpty(item.l.NewValue) ? "' '" : item.l.NewValue)
                    : string.Format("{0} User", item.l.Action);
                message += @"<tr>
                                <td>" + string.Format("{0:yyyy/MM/dd HH:mm}", item.l.Timestamp) + @"</td>
                                <td>" + string.Format("{0} {1} {2}", item.u.Firstname,item.u.Middlename,item.u.Lastname) + @"</td>
                                <td>" + description + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // GET: User
        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Reset = p.IsGranted(User.Identity.Name, "user_resetpassword");
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                List<UserViewModels> userViewModels = new List<UserViewModels>();
                bool isSetRole = p.IsGranted(User.Identity.Name, "user_setroles");
                if (isSetRole)
                {
                    foreach (var usr in db.User.ToList())
                    {
                        var roles = (from ur in db.UserRole
                                     join r in db.Role on ur.RoleId equals r.Id
                                     where ur.UserId == usr.Id
                                     orderby r.Name
                                     select new { r }).ToList();

                        userViewModels.Add(new UserViewModels
                        {
                            Id = usr.Id,
                            Fullname = usr.Firstname + " " + usr.Middlename + " " + usr.Lastname,
                            UserName = usr.UserName,
                            Email = usr.Email,
                            Roles = roles.Select(x => x.r.Name).ToList(),
                            Active = usr.Active
                        });
                    }
                    ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Name", "Name");
                    return View(userViewModels);
                }
                else
                {
                    string role_id_allowed = db.Settings.Find(SettingsValue.GUID_UserSetRoleAllowed).Value_Guid.Value.ToString();
                    var list_user = (from u in db.User
                                     join ur in db.UserRole on u.Id equals ur.UserId
                                     join r in db.Role on ur.RoleId equals r.Id
                                     where r.Id == role_id_allowed
                                     select new { u, ur, r }).ToList();
                    foreach (var usr in list_user)
                    {
                        userViewModels.Add(new UserViewModels
                        {
                            Id = usr.u.Id,
                            Fullname = usr.u.Firstname + " " + usr.u.Middlename + " " + usr.u.Lastname,
                            UserName = usr.u.UserName,
                            Email = usr.u.Email,
                            Roles = new List<string> { usr.r.Name },
                            Active = usr.u.Active
                        });
                    }
                    ViewBag.listRole = new SelectList(db.Role.Where(x => x.Id == role_id_allowed).OrderBy(x => x.Name).ToList(), "Name", "Name");
                    return View(userViewModels);
                }
            }
        }

        public ActionResult Edit(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                EditUserViewModels editUserViewModels = new EditUserViewModels
                {
                    User = db.User.Where(x => x.Id == id).FirstOrDefault(),
                    RoleId = db.UserRole.Where(x => x.UserId == id).Select(x => x.RoleId).ToList()
                };
                var interest_vm = editUserViewModels.User.Interest == null ? null : JsonConvert.DeserializeObject<List<InterestViewModels>>(editUserViewModels.User.Interest);
                if (interest_vm != null)
                {
                    editUserViewModels.LanguageId = interest_vm.Select(x => x.Languages_Id).ToList();
                }

                ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listLanguage = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listPromo = new SelectList(db.PromotionEvents.OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View(editUserViewModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "User,RoleId,LanguageId")] EditUserViewModels editUserViewModels)
        {
            if (ModelState.IsValid)
            {
                List<InterestViewModels> ivm = new List<InterestViewModels>();
                if (editUserViewModels.LanguageId != null)
                {
                    foreach (var i in editUserViewModels.LanguageId)
                    {
                        ivm.Add(new InterestViewModels
                        {
                            Languages_Id = i
                        });
                    }
                }
                string list_interest = editUserViewModels.LanguageId == null ? string.Empty : JsonConvert.SerializeObject(ivm);

                var current_data = db.User.Find(editUserViewModels.User.Id);
                current_data.Firstname = editUserViewModels.User.Firstname;
                current_data.Middlename = editUserViewModels.User.Middlename;
                current_data.Lastname = editUserViewModels.User.Lastname;
                current_data.Birthday = editUserViewModels.User.Birthday;
                current_data.UserName = editUserViewModels.User.UserName;
                current_data.Email = editUserViewModels.User.Email;
                current_data.Phone1 = editUserViewModels.User.Phone1;
                current_data.Phone2 = editUserViewModels.User.Phone2;
                current_data.Address = editUserViewModels.User.Address;
                current_data.Notes = editUserViewModels.User.Notes;
                current_data.Active = editUserViewModels.User.Active;
                current_data.Branches_Id = editUserViewModels.User.Branches_Id;
                current_data.PromotionEvents_Id = editUserViewModels.User.PromotionEvents_Id;
                current_data.Interest = list_interest;
                db.Entry(current_data).State = EntityState.Modified;
                db.SaveChanges();

                //var user = new UserModels()
                //{
                //    Id = editUserViewModels.User.Id,
                //    Firstname = editUserViewModels.User.Firstname,
                //    Middlename = editUserViewModels.User.Middlename,
                //    Lastname = editUserViewModels.User.Lastname,
                //    Birthday = editUserViewModels.User.Birthday,
                //    UserName = editUserViewModels.User.UserName,
                //    Email = editUserViewModels.User.Email,
                //    Phone1 = editUserViewModels.User.Phone1,
                //    Phone2 = editUserViewModels.User.Phone2,
                //    Address = editUserViewModels.User.Address,
                //    Notes = editUserViewModels.User.Notes,
                //    Active = editUserViewModels.User.Active,
                //    Branches_Id = editUserViewModels.User.Branches_Id
                //};
                //var userRole = new UserRoleModels() { UserId = userViewModels.Id, RoleId = userViewModels.RoleId };

                using (var database = new iSpeakContext())
                {
                    //database.User.Attach(user);
                    //database.Entry(user).Property(x => x.Firstname).IsModified = true;
                    //database.Entry(user).Property(x => x.Middlename).IsModified = true;
                    //database.Entry(user).Property(x => x.Lastname).IsModified = true;
                    //database.Entry(user).Property(x => x.Birthday).IsModified = true;
                    //database.Entry(user).Property(x => x.UserName).IsModified = true;
                    //database.Entry(user).Property(x => x.Email).IsModified = true;
                    //database.Entry(user).Property(x => x.Phone1).IsModified = true;
                    //database.Entry(user).Property(x => x.Phone2).IsModified = true;
                    //database.Entry(user).Property(x => x.Address).IsModified = true;
                    //database.Entry(user).Property(x => x.Notes).IsModified = true;
                    //database.Entry(user).Property(x => x.Active).IsModified = true;
                    //database.Entry(user).Property(x => x.Branches_Id).IsModified = true;

                    int deleteUserRole = database.Database.ExecuteSqlCommand("DELETE FROM AspNetUserRoles WHERE UserId='" + editUserViewModels.User.Id + "'");

                    //database.UserRole.Attach(userRole);
                    //database.Entry(userRole).Property(x => x.RoleId).IsModified = true;

                    //database.SaveChanges();
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
            ViewBag.listLanguage = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listPromo = new SelectList(db.PromotionEvents.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(editUserViewModels);
        }

        public async Task<ActionResult> Info(string id)
        {
            List<StudentPackageViewModels> student_packages = new List<StudentPackageViewModels>();
            var packages = await(from si in db.SaleInvoices
                                 join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                                 where si.Customer_UserAccounts_Id == id
                                 orderby si.Timestamp descending
                                 select new { si, u }).ToListAsync();
            foreach (var package in packages)
            {
                student_packages.Add(new StudentPackageViewModels
                {
                    SaleInvoices_Id = package.si.Id,
                    No = package.si.No,
                    Timestamp = TimeZoneInfo.ConvertTimeFromUtc(package.si.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    SaleInvoiceItems = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == package.si.Id).OrderBy(x => x.RowNo).ToList(),
                    StudentName = package.u.Firstname + " " + package.u.Middlename + " " + package.u.Lastname,
                    Amount = package.si.Amount,
                    Due = package.si.Due,
                    Cancelled = package.si.Cancelled
                });
            }
            return View(student_packages);
        }
    }
}