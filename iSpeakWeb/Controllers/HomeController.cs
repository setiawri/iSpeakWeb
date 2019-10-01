﻿using iSpeak.Models;
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
            Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
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

            var settings_rafr = await db.Settings.FindAsync(SettingsValue.GUID_RoleAccessForReminders);
            List<string> role_for_reminder = new List<string>();
            if (!string.IsNullOrEmpty(settings_rafr.Value_String))
            {
                string[] ids = settings_rafr.Value_String.Split(',');
                foreach (var id in ids)
                {
                    role_for_reminder.Add(id);
                }
            }
            var user_role = await (from u in db.User
                                         join ur in db.UserRole on u.Id equals ur.UserId
                                         join r in db.Role on ur.RoleId equals r.Id
                                         where u.UserName == User.Identity.Name
                                         select new { r }).ToListAsync();
            bool isAllowReminders = false;
            if (role_for_reminder.Count > 0)
            {
                foreach (var role in user_role)
                {
                    if (isAllowReminders) { break; }
                    else
                    {
                        foreach (var a in role_for_reminder)
                        {
                            if (a == role.r.Id)
                            {
                                isAllowReminders = true; break;
                            }
                        }
                    }
                }
            }


            BirthdayHome birthdayAlert = new BirthdayHome
            {
                Reminders = await db.Reminders.Where(x => x.Branches_Id == user_branch).ToListAsync(),
                ThisMonth = this_month,
                NextMonth = next_month,
                IsStudentBirthday = student_bday == null ? false : true,
                ShowBirthdayList = is_admin == null ? false : true,
                IsRemindersAllowed = isAllowReminders
            };

            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Name", "Name");
            return View(birthdayAlert);
        }
        
        #region Reminder Get
        public async Task<JsonResult> ReminderGet(Guid id)
        {
            var result = await db.Reminders.FindAsync(id);
            return Json(new { id = result.Id, timestamp = string.Format("{0:yyyy/MM/dd}", result.Timestamp), description = result.Description, status = (int)result.Status_enumid }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder Add
        public async Task<JsonResult> ReminderAdd(DateTime timestamp, string description)
        {
            RemindersModels remindersModels = new RemindersModels
            {
                Id = Guid.NewGuid(),
                Branches_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id,
                Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second),
                Description = description,
                Status_enumid = RemindersStatusEnum.New
            };
            db.Reminders.Add(remindersModels);

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder Edit
        public async Task<JsonResult> ReminderEdit(Guid id, DateTime timestamp, string description, int status)
        {
            RemindersModels remindersModels = await db.Reminders.FindAsync(id);
            remindersModels.Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
            remindersModels.Description = description;
            remindersModels.Status_enumid = (RemindersStatusEnum)status;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

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