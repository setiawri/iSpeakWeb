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
        private readonly iSpeakContext db = new iSpeakContext();

        #region GetPackages
        //public async Task<JsonResult> GetPackages(string user_id)
        //{
        //    List<StudentPackageViewModels> student_packages = new List<StudentPackageViewModels>();
        //    var packages = await(from si in db.SaleInvoices
        //                         join u in db.User on si.Customer_UserAccounts_Id equals u.Id
        //                         where si.Customer_UserAccounts_Id == user_id
        //                         orderby si.Timestamp descending
        //                         select new { si, u }).ToListAsync();
        //    foreach (var package in packages)
        //    {
        //        student_packages.Add(new StudentPackageViewModels
        //        {
        //            SaleInvoices_Id = package.si.Id,
        //            No = package.si.No,
        //            Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(package.si.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
        //            SaleInvoiceItems = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == package.si.Id).OrderBy(x => x.RowNo).ToList(),
        //            StudentName = package.u.Firstname + " " + package.u.Middlename + " " + package.u.Lastname,
        //            Amount = string.Format("{0:N0}", package.si.Amount),
        //            Due = string.Format("{0:N0}", package.si.Due),
        //            Cancelled = package.si.Cancelled
        //        });
        //    }

        //    return Json(new { obj = student_packages });
        //}
        #endregion

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

            var is_student = await (from u in db.User
                                    join ur in db.UserRole on u.Id equals ur.UserId
                                    join r in db.Role on ur.RoleId equals r.Id
                                    where r.Name.ToLower() == "student" && u.UserName == User.Identity.Name
                                    select new { u }).FirstOrDefaultAsync();

            List<StudentPackageViewModels> student_packages = new List<StudentPackageViewModels>();
            if (is_student != null)
            {
                var packages = await (from si in db.SaleInvoices
                                      join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                                      where u.UserName == User.Identity.Name
                                      orderby si.Timestamp descending
                                      select new { si, u }).ToListAsync();
                foreach (var package in packages)
                {
                    var sii_list = await db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == package.si.Id && x.LessonPackages_Id != null).ToListAsync();
                    if (sii_list.Count > 0)
                    {
                        student_packages.Add(new StudentPackageViewModels
                        {
                            SaleInvoices_Id = package.si.Id,
                            No = package.si.No,
                            Timestamp = TimeZoneInfo.ConvertTimeFromUtc(package.si.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                            SaleInvoiceItems = db.SaleInvoiceItems.Where(x => x.SaleInvoices_Id == package.si.Id && x.LessonPackages_Id != null).OrderBy(x => x.RowNo).ToList(),
                            StudentName = package.u.Firstname + " " + package.u.Middlename + " " + package.u.Lastname,
                            Amount = package.si.Amount,
                            Due = package.si.Due,
                            Cancelled = package.si.Cancelled
                        });
                    }
                }
            }

            var student_schedule = await (from tss in db.TutorStudentSchedules
                                          join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                                          join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                                          join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                                          where s.UserName == User.Identity.Name && tss.IsActive == true
                                          orderby tss.DayOfWeek
                                          select new TutorStudentSchedulesViewModels
                                          {
                                              Id = tss.Id,
                                              Tutor = t.Firstname + " " + t.Middlename + " " + t.Lastname,
                                              Student = s.Firstname + " " + s.Middlename + " " + s.Lastname,
                                              DayOfWeek = tss.DayOfWeek,
                                              StartTime = tss.StartTime,
                                              EndTime = tss.EndTime,
                                              Invoice = sii.Description,
                                              IsActive = tss.IsActive,
                                              Notes = tss.Notes
                                          }).ToListAsync();

            BirthdayHome birthdayAlert = new BirthdayHome
            {
                Reminders = await db.Reminders.Where(x => x.Branches_Id == user_login.Branches_Id && x.Status_enumid != RemindersStatusEnum.Completed && x.Status_enumid != RemindersStatusEnum.Cancel).ToListAsync(),
                ThisMonth = this_month,
                NextMonth = next_month,
                StudentPackages = student_packages,
                StudentSchedules = student_schedule,
                IsStudentBirthday = student_bday == null ? false : true,
                ShowBirthdayList = is_admin == null ? false : true,
                IsRemindersAllowed = isAllowReminders,
                ShowStudentPackage = is_student == null ? false : true,
                ShowStudentSchedule = is_student == null ? false : true
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

            string status;
            DateTime dateEnd = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
            DateTime dateStart = dateEnd.AddSeconds(-10); //range time = 10 seconds
            var reminderCheck = await db.Reminders.Where(x => x.Branches_Id == remindersModels.Branches_Id
                                                            && x.Timestamp >= dateStart && x.Timestamp <= dateEnd
                                                            && x.Description == remindersModels.Description
                                                            && x.Status_enumid == remindersModels.Status_enumid
                                                        ).FirstOrDefaultAsync();
            if (reminderCheck != null) { status = "300"; }
            else
            {
                status = "200";
                db.Reminders.Add(remindersModels);

                ActivityLogsModels activityLogsModels = new ActivityLogsModels
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    TableName = "Reminders",
                    RefId = remindersModels.Id,
                    Description = "[New] " + description,
                    UserAccounts_Id = user_login.Id
                };
                db.ActivityLogs.Add(activityLogsModels);

                await db.SaveChangesAsync();
            }

            return Json(new { status }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder Update
        public async Task<JsonResult> ReminderUpdate(Guid id, DateTime timestamp, string description, int status)
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();

            RemindersModels remindersModels = await db.Reminders.FindAsync(id);
            //remindersModels.Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
            //remindersModels.Description = description;
            remindersModels.Status_enumid = (RemindersStatusEnum)status;
            db.Entry(remindersModels).State = EntityState.Modified;

            string desc_log = "";
            if (remindersModels.Status_enumid == RemindersStatusEnum.New)
            {
                desc_log = "[New] " + description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.InProgress)
            {
                desc_log = "[In Progress] " + description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.OnHold)
            {
                desc_log = "[On Hold] " + description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Waiting)
            {
                desc_log = "[Waiting] " + description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Completed)
            {
                desc_log = "[Completed] " + description;
            }
            else if (remindersModels.Status_enumid == RemindersStatusEnum.Cancel)
            {
                desc_log = "[Cancel] " + description;
            }

            ActivityLogsModels activityLogsModels = new ActivityLogsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
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
                                <td>" + string.Format("{0:yyyy/MM/dd HH:mm:ss}", TimeZoneInfo.ConvertTimeFromUtc(item.al.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))) + @"</td>
                                <td>" + item.al.Description + @"</td>
                                <td>" + item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Reminder By Status
        public async Task<JsonResult> ReminderByStatus(string key)
        {
            var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
            var reminders = key == "all"
                ? await db.Reminders.Where(x => x.Branches_Id == user_login.Branches_Id).ToListAsync()
                : await db.Reminders.Where(x => x.Branches_Id == user_login.Branches_Id
                    && x.Status_enumid != RemindersStatusEnum.Completed 
                    && x.Status_enumid != RemindersStatusEnum.Cancel).ToListAsync();

            List<RemindersViewModels> list = new List<RemindersViewModels>();
            foreach(var reminder in reminders)
            {
                string status_render = "";
                if (reminder.Status_enumid == RemindersStatusEnum.New)
                {
                    status_render = "<span class='badge badge-dark d-block'>New</span>";
                }
                else if (reminder.Status_enumid == RemindersStatusEnum.InProgress)
                {
                    status_render = "<span class='badge badge-info d-block'>In Progress</span>";
                }
                else if (reminder.Status_enumid == RemindersStatusEnum.OnHold)
                {
                    status_render = "<span class='badge bg-brown d-block'>On Hold</span>";
                }
                else if (reminder.Status_enumid == RemindersStatusEnum.Waiting)
                {
                    status_render = "<span class='badge badge-warning d-block'>Waiting</span>";
                }
                else if (reminder.Status_enumid == RemindersStatusEnum.Completed)
                {
                    status_render = "<span class='badge badge-primary d-block'>Completed</span>";
                }
                else if (reminder.Status_enumid == RemindersStatusEnum.Cancel)
                {
                    status_render = "<span class='badge badge-danger d-block'>Cancel</span>";
                }

                list.Add(new RemindersViewModels
                {
                    Id = reminder.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd}", reminder.Timestamp),
                    Description = reminder.Description,
                    Status = status_render,
                    Action_Render = "<a href='#' onclick='Update_Reminder(\"" + reminder.Id + "\")'>Update</a> | <a href='#' onclick='View_Log(\"" + reminder.Id + "\")'>Log</a>"
                });
            }

            return Json(new { obj = list }, JsonRequestBehavior.AllowGet);
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