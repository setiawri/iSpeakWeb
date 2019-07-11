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
        private iSpeakContext db = new iSpeakContext();

        public ActionResult Index()
        {
            //var data2 = (from hr in db.HourlyRates
            //            join lp in db.LessonPackages on hr.LessonPackages_Id equals lp.Id
            //            join u in db.User on hr.UserAccounts_Id equals u.Id
            //            orderby lp.Name
            //            select new HourlyRatesViewModels
            //            {
            //                Id = hr.Id,
            //                LessonPackages = lp.Name,
            //                UserAccounts = u.Firstname + " " + u.Middlename + " " + u.Lastname,
            //                Rate = hr.Rate,
            //                Notes = hr.Notes
            //            }).ToListAsync();

            var data = (from hr in db.HourlyRates
                        join u in db.User on hr.UserAccounts_Id equals u.Id
                        select new { hr, u }).ToList();
            List<HourlyRatesViewModels> list = new List<HourlyRatesViewModels>();
            foreach (var item in data)
            {
                HourlyRatesViewModels model = new HourlyRatesViewModels();
                model.Id = item.hr.Id;
                model.LessonPackages = (item.hr.LessonPackages_Id.HasValue) ? db.LessonPackages.Where(x => x.Id == item.hr.LessonPackages_Id).FirstOrDefault().Name : string.Empty;
                model.UserAccounts = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname;
                model.Rate = item.hr.Rate;
                model.Notes = item.hr.Notes;
                list.Add(model);
            }
            return View(list);
        }

        public ActionResult Create()
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
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listUser = new SelectList(user_list, "Id", "Name");

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
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
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
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listUser = new SelectList(user_list, "Id", "Name");

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

        public async Task<ActionResult> Edit(Guid? id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hourlyRatesModels).State = EntityState.Modified;
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
            var data = (from hr in db.HourlyRates
                        join lp in db.LessonPackages on hr.LessonPackages_Id equals lp.Id
                        join u in db.User on hr.UserAccounts_Id equals u.Id
                        where hr.Id == id
                        select new HourlyRatesViewModels
                        {
                            Id = hr.Id,
                            LessonPackages = lp.Name,
                            UserAccounts = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Rate = hr.Rate,
                            Notes = hr.Notes
                        }).FirstOrDefaultAsync();
            return View(await data);
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