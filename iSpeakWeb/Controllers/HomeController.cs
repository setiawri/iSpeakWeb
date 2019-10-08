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
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            //Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
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
                Reminders = await db.Reminders.Where(x => x.Branches_Id == user_login.Branches_Id).ToListAsync(),
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
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();

            RemindersModels remindersModels = new RemindersModels
            {
                Id = Guid.NewGuid(),
                Branches_Id = user_login.Branches_Id,
                Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second),
                Description = description,
                Status_enumid = RemindersStatusEnum.New
            };
            db.Reminders.Add(remindersModels);

            string desc_log = "";
            if (remindersModels.Status_enumid == RemindersStatusEnum.New)
            {
                desc_log = "[New] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.InProgress)
            {
                desc_log = "[In Progress] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.OnHold)
            {
                desc_log = "[On Hold] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Waiting)
            {
                desc_log = "[Waiting] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Completed)
            {
                desc_log = "[Completed] " + remindersModels.Description;
            }

            ActivityLogsModels activityLogsModels = new ActivityLogsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = remindersModels.Timestamp,
                TableName = "Reminders",
                RefId = remindersModels.Id,
                Description = desc_log,
                UserAccounts_Id = user_login.Id
            };
            db.ActivityLogs.Add(activityLogsModels);

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder Update
        public async Task<JsonResult> ReminderUpdate(Guid id, DateTime timestamp, string description, int status)
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();

            RemindersModels remindersModels = await db.Reminders.FindAsync(id);
            remindersModels.Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
            remindersModels.Description = description;
            remindersModels.Status_enumid = (RemindersStatusEnum)status;
            db.Entry(remindersModels).State = EntityState.Modified;

            string desc_log = "";
            if (remindersModels.Status_enumid == RemindersStatusEnum.New)
            {
                desc_log = "[New] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.InProgress)
            {
                desc_log = "[In Progress] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.OnHold)
            {
                desc_log = "[On Hold] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Waiting)
            {
                desc_log = "[Waiting] " + remindersModels.Description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Completed)
            {
                desc_log = "[Completed] " + remindersModels.Description;
            }

            ActivityLogsModels activityLogsModels = new ActivityLogsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = remindersModels.Timestamp,
                TableName = "Reminders",
                RefId = remindersModels.Id,
                Description = desc_log,
                UserAccounts_Id = user_login.Id
            };
            db.ActivityLogs.Add(activityLogsModels);

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder Log
        public JsonResult ReminderLog(Guid id)
        {
            var list = (from al in db.ActivityLogs
                        join r in db.Reminders on al.RefId equals r.Id
                        join u in db.User on al.UserAccounts_Id equals u.Id
                        where al.RefId == id
                        orderby al.Timestamp descending
                        select new { al, r, u }).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Description</th>
                                                <th>Created By</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>" + string.Format("{0:yyyy/MM/dd}", item.al.Timestamp) + @"</td>
                                <td>" + item.al.Description + @"</td>
                                <td>" + item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
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