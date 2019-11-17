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
    [Authorize]
    public class HourlyRatesController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var data = await (from hr in db.HourlyRates
                                  join u in db.User on hr.UserAccounts_Id equals u.Id
                                  select new { hr, u }).ToListAsync();
                List<HourlyRatesViewModels> list = new List<HourlyRatesViewModels>();
                foreach (var item in data)
                {
                    list.Add(new HourlyRatesViewModels
                    {
                        Id = item.hr.Id,
                        Branch = (item.hr.Branches_Id.HasValue) ? db.Branches.Where(x => x.Id == item.hr.Branches_Id).FirstOrDefault().Name : string.Empty,
                        LessonPackages = (item.hr.LessonPackages_Id.HasValue) ? db.LessonPackages.Where(x => x.Id == item.hr.LessonPackages_Id).FirstOrDefault().Name : string.Empty,
                        UserAccounts = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                        Rate = item.hr.Rate,
                        Notes = item.hr.Notes
                    });
                }
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var users = (from u in db.User
                             join ur in db.UserRole on u.Id equals ur.UserId
                             join r in db.Role on ur.RoleId equals r.Id
                             where r.Name == "Tutor"
                             orderby u.Firstname
                             select new { u }).ToList();
                List<object> user_list = new List<object>();
                foreach (var item in users)
                {
                    user_list.Add(new
                    {
                        item.u.Id,
                        Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                    });
                }
                ViewBag.listUser = new SelectList(user_list, "Id", "Name");

                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

                var lessons = (from lp in db.LessonPackages
                               join l in db.Languages on lp.Languages_Id equals l.Id
                               join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                               where lp.Active == true
                               select new LessonPackagesViewModels
                               {
                                   Id = lp.Id,
                                   Name = lp.Name,
                                   Languages = l.Name,
                                   LessonTypes = lt.Name,
                                   SessionHours = lp.SessionHours,
                                   ExpirationDay = lp.ExpirationDay,
                                   Price = lp.Price,
                                   Active = lp.Active
                               }).ToList();
                List<object> lesson_list = new List<object>();
                foreach (var item in lessons)
                {
                    lesson_list.Add(new
                    {
                        item.Id,
                        Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + " days, " + item.Price.ToString("#,##0") + ")"
                    });
                }
                ViewBag.listLessonPackage = new SelectList(lesson_list, "Id", "Name");

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
        {
            if (ModelState.IsValid)
            {
                hourlyRatesModels.Id = Guid.NewGuid();
                db.HourlyRates.Add(hourlyRatesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var users = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where r.Name == "Tutor"
                         orderby u.Firstname
                         select new { u }).ToList();
            List<object> user_list = new List<object>();
            foreach (var item in users)
            {
                user_list.Add(new
                {
                    item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listUser = new SelectList(user_list, "Id", "Name");

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

            var lessons = (from lp in db.LessonPackages
                           join l in db.Languages on lp.Languages_Id equals l.Id
                           join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                           where lp.Active == true
                           select new LessonPackagesViewModels
                           {
                               Id = lp.Id,
                               Name = lp.Name,
                               Languages = l.Name,
                               LessonTypes = lt.Name,
                               SessionHours = lp.SessionHours,
                               ExpirationDay = lp.ExpirationDay,
                               Price = lp.Price,
                               Active = lp.Active
                           }).ToList();
            List<object> lesson_list = new List<object>();
            foreach (var item in lessons)
            {
                lesson_list.Add(new
                {
                    item.Id,
                    Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + " days, " + item.Price.ToString("#,##0") + ")"
                });
            }
            ViewBag.listLessonPackage = new SelectList(lesson_list, "Id", "Name");

            return View(hourlyRatesModels);
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
                HourlyRatesModels hourlyRatesModels = await db.HourlyRates.FindAsync(id);
                if (hourlyRatesModels == null)
                {
                    return HttpNotFound();
                }

                var users = (from u in db.User
                             join ur in db.UserRole on u.Id equals ur.UserId
                             join r in db.Role on ur.RoleId equals r.Id
                             where r.Name == "Tutor"
                             orderby u.Firstname
                             select new { u }).ToList();
                List<object> user_list = new List<object>();
                foreach (var item in users)
                {
                    user_list.Add(new
                    {
                        Id = item.u.Id,
                        Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                    });
                }
                ViewBag.listUser = new SelectList(user_list, "Id", "Name");

                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

                var lessons = (from lp in db.LessonPackages
                               join l in db.Languages on lp.Languages_Id equals l.Id
                               join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                               where lp.Active == true
                               select new LessonPackagesViewModels
                               {
                                   Id = lp.Id,
                                   Name = lp.Name,
                                   Languages = l.Name,
                                   LessonTypes = lt.Name,
                                   SessionHours = lp.SessionHours,
                                   ExpirationDay = lp.ExpirationDay,
                                   Price = lp.Price,
                                   Active = lp.Active
                               }).ToList();
                List<object> lesson_list = new List<object>();
                foreach (var item in lessons)
                {
                    lesson_list.Add(new
                    {
                        Id = item.Id,
                        Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + " days, " + item.Price.ToString("#,##0") + ")"
                    });
                }
                ViewBag.listLessonPackage = new SelectList(lesson_list, "Id", "Name");

                return View(hourlyRatesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.HourlyRates.FindAsync(hourlyRatesModels.Id);
                current_data.Branches_Id = hourlyRatesModels.Branches_Id;
                current_data.LessonPackages_Id = hourlyRatesModels.LessonPackages_Id;
                current_data.UserAccounts_Id = hourlyRatesModels.UserAccounts_Id;
                current_data.Rate = hourlyRatesModels.Rate;
                current_data.Notes = hourlyRatesModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var users = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where r.Name == "Tutor"
                         orderby u.Firstname
                         select new { u }).ToList();
            List<object> user_list = new List<object>();
            foreach (var item in users)
            {
                user_list.Add(new
                {
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listUser = new SelectList(user_list, "Id", "Name");

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

            var lessons = (from lp in db.LessonPackages
                           join l in db.Languages on lp.Languages_Id equals l.Id
                           join lt in db.LessonTypes on lp.LessonTypes_Id equals lt.Id
                           where lp.Active == true
                           select new LessonPackagesViewModels
                           {
                               Id = lp.Id,
                               Name = lp.Name,
                               Languages = l.Name,
                               LessonTypes = lt.Name,
                               SessionHours = lp.SessionHours,
                               ExpirationDay = lp.ExpirationDay,
                               Price = lp.Price,
                               Active = lp.Active
                           }).ToList();
            List<object> lesson_list = new List<object>();
            foreach (var item in lessons)
            {
                lesson_list.Add(new
                {
                    Id = item.Id,
                    Name = "[" + item.LessonTypes + ", " + item.Languages + "] " + item.Name + " (" + item.SessionHours + " hrs, " + item.ExpirationDay + " days, " + item.Price.ToString("#,##0") + ")"
                });
            }
            ViewBag.listLessonPackage = new SelectList(lesson_list, "Id", "Name");

            return View(hourlyRatesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                //var data = (from hr in db.HourlyRates
                //            join b in db.Branches on hr.Branches_Id equals b.Id
                //            join lp in db.LessonPackages on hr.LessonPackages_Id equals lp.Id
                //            join u in db.User on hr.UserAccounts_Id equals u.Id
                //            where hr.Id == id
                //            select new HourlyRatesViewModels
                //            {
                //                Id = hr.Id,
                //                Branch = hr.Branches_Id.HasValue ? b.Name : "",
                //                LessonPackages = lp.Name,
                //                UserAccounts = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                //                Rate = hr.Rate,
                //                Notes = hr.Notes
                //            }).FirstOrDefaultAsync();
                var item = await (from hr in db.HourlyRates
                                  join u in db.User on hr.UserAccounts_Id equals u.Id
                                  where hr.Id == id
                                  select new { hr, u }).FirstOrDefaultAsync();
                HourlyRatesViewModels model = new HourlyRatesViewModels
                {
                    Id = item.hr.Id,
                    Branch = (item.hr.Branches_Id.HasValue) ? db.Branches.Where(x => x.Id == item.hr.Branches_Id).FirstOrDefault().Name : string.Empty,
                    LessonPackages = (item.hr.LessonPackages_Id.HasValue) ? db.LessonPackages.Where(x => x.Id == item.hr.LessonPackages_Id).FirstOrDefault().Name : string.Empty,
                    UserAccounts = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                    Rate = item.hr.Rate,
                    Notes = item.hr.Notes
                };

                return View(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            HourlyRatesModels hourlyRatesModels = await db.HourlyRates.FindAsync(id);
            db.HourlyRates.Remove(hourlyRatesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}