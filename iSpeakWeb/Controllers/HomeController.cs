using iSpeak.Models;
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
    public class HomeController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            var date_now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            #region Birthday List
            var this_month = await (from u in db.User
                                    join ur in db.UserRole on u.Id equals ur.UserId
                                    join r in db.Role on ur.RoleId equals r.Id
                                    where u.Birthday.Month == date_now.Month && u.Birthday.Day >= date_now.Day
                                    select new BirthdayViewModels
                                    {
                                        Fullname = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                        Birthday = u.Birthday,
                                        Role = r.Name,
                                        CountDay = u.Birthday.Day - date_now.Day
                                    }).ToListAsync();

            var date_next = date_now.AddMonths(1);
            var next_month_linq = await (from u in db.User
                                    join ur in db.UserRole on u.Id equals ur.UserId
                                    join r in db.Role on ur.RoleId equals r.Id
                                    where u.Birthday.Month == date_next.Month
                                    select new { u, r }).ToListAsync();

            List<BirthdayViewModels> next_month = new List<BirthdayViewModels>();
            foreach (var item in next_month_linq)
            {
                DateTime birthday_this_year = new DateTime(date_next.Year, item.u.Birthday.Month, item.u.Birthday.Day);
                next_month.Add(new BirthdayViewModels
                {
                    Fullname = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                    Birthday = item.u.Birthday,
                    Role = item.r.Name,
                    CountDay = (birthday_this_year - date_now).Days + 1
                });
            }
            #endregion
            var student_bday = await (from u in db.User
                                      join ur in db.UserRole on u.Id equals ur.UserId
                                      join r in db.Role on ur.RoleId equals r.Id
                                      where r.Name.ToLower() == "student" && u.UserName == User.Identity.Name
                                        && u.Birthday.Month == date_now.Month && u.Birthday.Day == date_now.Day
                                      select new { u }).FirstOrDefaultAsync();

            var is_admin = await (from u in db.User
                                  join ur in db.UserRole on u.Id equals ur.UserId
                                  join r in db.Role on ur.RoleId equals r.Id
                                  where r.Name.ToLower() == "admin" && u.UserName == User.Identity.Name
                                  select new { u }).FirstOrDefaultAsync();

            BirthdayHome birthdayAlert = new BirthdayHome
            {
                ThisMonth = this_month,
                NextMonth = next_month,
                IsStudentBirthday = student_bday == null ? false : true,
                ShowBirthdayList = is_admin == null ? false : true
            };

            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Name", "Name");
            return View(birthdayAlert);
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}