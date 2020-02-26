using iSpeak.Common;
using iSpeak.Models;
using Newtonsoft.Json;
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
    public class SchedulesController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region TUTOR
        public async Task<JsonResult> GetTutorSchedule(string tutor_id)
        {
            var items = await (from ts in db.TutorSchedules
                               join u in db.User on ts.Tutor_UserAccounts_Id equals u.Id
                               where ts.Tutor_UserAccounts_Id == tutor_id
                               orderby ts.DayOfWeek, ts.StartTime
                               select new TutorSchedulesViewModels
                               {
                                   Id = ts.Id,
                                   Tutor = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                   DayOfWeek = ts.DayOfWeek,
                                   StartTime = ts.StartTime,
                                   EndTime = ts.EndTime,
                                   IsActive = ts.IsActive,
                                   Notes = ts.Notes
                               }).ToListAsync();
            
            string content = "";
            foreach (var item in items)
            {
                content += "<tr>";
                content += "<td>" + item.DayOfWeek + "</td>";
                content += "<td>" + string.Format("{0:HH:mm} - {1:HH:mm}", item.StartTime, item.EndTime) + "</td>";
                content += "<td>" + item.Notes + "</td>";
                content += "<td><a href=javascript:void(0) onclick='DeleteSchedule(\"" + item.Id + "\",\"" + item.Tutor + "\",\"" + item.DayOfWeek + "\",\"" + string.Format("{0:HH:mm} - {1:HH:mm}", item.StartTime, item.EndTime) + "\")'>Delete</a></td>";
                content += "</tr>";
            }

            return Json(new { body = content }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> TutorIndex()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                #region CHECK FULL ACCESS
                var setting_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                List<string> role_for_tutor_schedule = new List<string>();
                if (!string.IsNullOrEmpty(setting_fafts.Value_String))
                {
                    string[] ids = setting_fafts.Value_String.Split(',');
                    foreach (var _id in ids)
                    {
                        role_for_tutor_schedule.Add(_id);
                    }
                }

                var user_role = await (from u in db.User
                                       join ur in db.UserRole on u.Id equals ur.UserId
                                       join r in db.Role on ur.RoleId equals r.Id
                                       where u.UserName == User.Identity.Name
                                       select new { r }).ToListAsync();
                bool isFullAccess = false;
                if (role_for_tutor_schedule.Count > 0)
                {
                    foreach (var role in user_role)
                    {
                        if (isFullAccess) { break; }
                        else
                        {
                            foreach (var a in role_for_tutor_schedule)
                            {
                                if (a == role.r.Id)
                                {
                                    isFullAccess = true; break;
                                }
                            }
                        }
                    }
                }
                #endregion

                var list = (isFullAccess)
                    ? await (from ts in db.TutorSchedules
                             join u in db.User on ts.Tutor_UserAccounts_Id equals u.Id
                             select new TutorSchedulesViewModels
                             {
                                 Id = ts.Id,
                                 Tutor = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                 DayOfWeek = ts.DayOfWeek,
                                 StartTime = ts.StartTime,
                                 EndTime = ts.EndTime,
                                 IsActive = ts.IsActive,
                                 Notes = ts.Notes
                             }).ToListAsync()
                    : await (from ts in db.TutorSchedules
                             join u in db.User on ts.Tutor_UserAccounts_Id equals u.Id
                             where u.UserName == User.Identity.Name
                             select new TutorSchedulesViewModels
                             {
                                 Id = ts.Id,
                                 Tutor = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                 DayOfWeek = ts.DayOfWeek,
                                 StartTime = ts.StartTime,
                                 EndTime = ts.EndTime,
                                 IsActive = ts.IsActive,
                                 Notes = ts.Notes
                             }).ToListAsync();

                //ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public async Task<ActionResult> TutorCreate(string id, string dow, string start, string end)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                #region CHECK FULL ACCESS
                var setting_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                List<string> role_for_tutor_schedule = new List<string>();
                if (!string.IsNullOrEmpty(setting_fafts.Value_String))
                {
                    string[] ids = setting_fafts.Value_String.Split(',');
                    foreach (var _id in ids)
                    {
                        role_for_tutor_schedule.Add(_id);
                    }
                }

                var user_role = await (from u in db.User
                                       join ur in db.UserRole on u.Id equals ur.UserId
                                       join r in db.Role on ur.RoleId equals r.Id
                                       where u.UserName == User.Identity.Name
                                       select new { r }).ToListAsync();
                bool isFullAccess = false;
                if (role_for_tutor_schedule.Count > 0)
                {
                    foreach (var role in user_role)
                    {
                        if (isFullAccess) { break; }
                        else
                        {
                            foreach (var a in role_for_tutor_schedule)
                            {
                                if (a == role.r.Id)
                                {
                                    isFullAccess = true; break;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (!isFullAccess)
                {
                    var user_login = await db.User.Where(x => x.UserName.ToLower() == User.Identity.Name.ToLower()).FirstOrDefaultAsync();
                    ViewBag.LoginId = user_login.Id;
                    ViewBag.LoginFullName = user_login.Firstname + " " + user_login.Middlename + " " + user_login.Lastname;
                }

                if (!string.IsNullOrEmpty(id))
                {
                    var tutor = await db.User.FindAsync(id);
                    ViewBag.Error = "init";
                    ViewBag.TutorId = tutor == null ? "" : tutor.Id;
                    ViewBag.TutorName = tutor == null ? "" : tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
                }

                ViewBag.DOW = string.IsNullOrEmpty(dow) ? "0" : dow;
                ViewBag.StartTime = string.IsNullOrEmpty(start) ? string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 8, 0, 0)) : start.Replace("_", ":");
                ViewBag.EndTime = string.IsNullOrEmpty(end) ? string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 12, 0, 0)) : end.Replace("_", ":");
                ViewBag.FullAccess = isFullAccess;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TutorCreate([Bind(Include = "Id,Tutor_UserAccounts_Id,DayOfWeek,StartTime,EndTime,IsActive,Notes")] TutorSchedulesModels tutorSchedulesModels)
        {
            string message_error = "";

            var start = new DateTime(1970, 01, 01, tutorSchedulesModels.StartTime.Hour, tutorSchedulesModels.StartTime.Minute, 0);
            var end = new DateTime(1970, 01, 01, tutorSchedulesModels.EndTime.Hour, tutorSchedulesModels.EndTime.Minute, 0);

            var isExist = await db.TutorSchedules
                .Where(x => x.Tutor_UserAccounts_Id == tutorSchedulesModels.Tutor_UserAccounts_Id
                    && x.DayOfWeek == tutorSchedulesModels.DayOfWeek
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();
            if (isExist != null)
            {
                var _tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                string tutor_name = _tutor.Firstname + " " + _tutor.Middlename + " " + _tutor.Lastname;
                ModelState.AddModelError("Exist", "This schedule already exist ( " + string.Format("{0}: {1}, {2:HH:mm} - {3:HH:mm}", tutor_name, tutorSchedulesModels.DayOfWeek, start, end) + " ).");
                message_error = "duplicate_schedule";
                ViewBag.Error = message_error;
            }

            if (tutorSchedulesModels.StartTime.Hour > tutorSchedulesModels.EndTime.Hour
                || (tutorSchedulesModels.StartTime.Hour == tutorSchedulesModels.EndTime.Hour
                    && tutorSchedulesModels.StartTime.Minute > tutorSchedulesModels.EndTime.Minute))
            {
                ModelState.AddModelError("Schedule", "The Start Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.StartTime) + " ) field cannot greater than End Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.EndTime) + " ) field.");
                message_error = "invalid_time";
                ViewBag.Error = message_error;
            }

            if (ModelState.IsValid)
            {
                tutorSchedulesModels.Id = Guid.NewGuid();
                tutorSchedulesModels.StartTime = start;
                tutorSchedulesModels.EndTime = end;
                tutorSchedulesModels.IsActive = true;
                db.TutorSchedules.Add(tutorSchedulesModels);
                await db.SaveChangesAsync();

                return RedirectToAction("TutorCreate", new { id = tutorSchedulesModels.Tutor_UserAccounts_Id, dow = (int)tutorSchedulesModels.DayOfWeek, start = string.Format("{0:HH_mm}", start), end = string.Format("{0:HH_mm}", end) });
                //return RedirectToAction("TutorIndex");
            }

            var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
            ViewBag.TutorId = tutor == null ? "" : tutor.Id;
            ViewBag.TutorName = tutor == null ? "" : tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
            ViewBag.StartTime = string.Format("{0:HH:mm}", start);
            ViewBag.EndTime = string.Format("{0:HH:mm}", end);
            return View(tutorSchedulesModels);
        }

        public async Task<ActionResult> TutorEdit(Guid? id)
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
                TutorSchedulesModels tutorSchedulesModels = await db.TutorSchedules.FindAsync(id);
                if (tutorSchedulesModels == null)
                {
                    return HttpNotFound();
                }

                #region CHECK FULL ACCESS
                var setting_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                List<string> role_for_tutor_schedule = new List<string>();
                if (!string.IsNullOrEmpty(setting_fafts.Value_String))
                {
                    string[] ids = setting_fafts.Value_String.Split(',');
                    foreach (var _id in ids)
                    {
                        role_for_tutor_schedule.Add(_id);
                    }
                }

                var user_role = await (from u in db.User
                                       join ur in db.UserRole on u.Id equals ur.UserId
                                       join r in db.Role on ur.RoleId equals r.Id
                                       where u.UserName == User.Identity.Name
                                       select new { r }).ToListAsync();
                bool isFullAccess = false;
                if (role_for_tutor_schedule.Count > 0)
                {
                    foreach (var role in user_role)
                    {
                        if (isFullAccess) { break; }
                        else
                        {
                            foreach (var a in role_for_tutor_schedule)
                            {
                                if (a == role.r.Id)
                                {
                                    isFullAccess = true; break;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (!isFullAccess)
                {
                    var user_login = await db.User.Where(x => x.UserName.ToLower() == User.Identity.Name.ToLower()).FirstOrDefaultAsync();
                    ViewBag.LoginId = user_login.Id;
                    ViewBag.LoginFullName = user_login.Firstname + " " + user_login.Middlename + " " + user_login.Lastname;
                }

                var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                ViewBag.TutorName = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
                ViewBag.FullAccess = isFullAccess;
                return View(tutorSchedulesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TutorEdit([Bind(Include = "Id,Tutor_UserAccounts_Id,DayOfWeek,StartTime,EndTime,IsActive,Notes")] TutorSchedulesModels tutorSchedulesModels)
        {
            var start = new DateTime(1970, 01, 01, tutorSchedulesModels.StartTime.Hour, tutorSchedulesModels.StartTime.Minute, 0);
            var end = new DateTime(1970, 01, 01, tutorSchedulesModels.EndTime.Hour, tutorSchedulesModels.EndTime.Minute, 0);

            var isExist = await db.TutorSchedules
                .Where(x => x.Id != tutorSchedulesModels.Id
                    && x.Tutor_UserAccounts_Id == tutorSchedulesModels.Tutor_UserAccounts_Id
                    && x.DayOfWeek == tutorSchedulesModels.DayOfWeek
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();
            if (isExist != null)
            {
                var _tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                string tutor_name = _tutor.Firstname + " " + _tutor.Middlename + " " + _tutor.Lastname;
                ModelState.AddModelError("Exist", "This schedule already exist ( " + string.Format("{0}: {1}, {2:HH:mm} - {3:HH:mm}", tutor_name, tutorSchedulesModels.DayOfWeek, start, end) + " ).");
            }

            if (tutorSchedulesModels.StartTime.Hour > tutorSchedulesModels.EndTime.Hour
                || (tutorSchedulesModels.StartTime.Hour == tutorSchedulesModels.EndTime.Hour
                    && tutorSchedulesModels.StartTime.Minute > tutorSchedulesModels.EndTime.Minute))
            {
                ModelState.AddModelError("Schedule", "The Start Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.StartTime) + " ) field cannot greater than End Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.EndTime) + " ) field.");
            }

            if (ModelState.IsValid)
            {
                var current_data = await db.TutorSchedules.FindAsync(tutorSchedulesModels.Id);
                current_data.Tutor_UserAccounts_Id = tutorSchedulesModels.Tutor_UserAccounts_Id;
                current_data.DayOfWeek = tutorSchedulesModels.DayOfWeek;
                current_data.StartTime = start;
                current_data.EndTime = end;
                current_data.IsActive = tutorSchedulesModels.IsActive;
                current_data.Notes = tutorSchedulesModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("TutorIndex");
            }

            var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
            ViewBag.TutorName = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
            return View(tutorSchedulesModels);
        }
        #endregion
        #region STUDENT
        public async Task<JsonResult> GetStudentSchedule(string student_id, string tutor_id)
        {
            var items = string.IsNullOrEmpty(tutor_id)
                ? await (from tss in db.TutorStudentSchedules
                         join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                         join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                         join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                         where tss.Student_UserAccounts_Id == student_id
                         orderby tss.DayOfWeek, tss.StartTime
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
                         }).ToListAsync()
                : await (from tss in db.TutorStudentSchedules
                         join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                         join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                         join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                         where tss.Student_UserAccounts_Id == student_id && tss.Tutor_UserAccounts_Id == tutor_id
                         orderby tss.DayOfWeek, tss.StartTime
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

            string content = "";
            foreach (var item in items)
            {
                content += "<tr>";
                content += "<td>" + item.DayOfWeek + "</td>";
                content += "<td>" + string.Format("{0:HH:mm} - {1:HH:mm}", item.StartTime, item.EndTime) + "</td>";
                content += "<td>" + item.Invoice + "</td>";
                content += "<td>" + item.Tutor + "</td>";
                content += "<td>" + item.Notes + "</td>";
                content += "<td><a href=javascript:void(0) onclick='DeleteSchedule(\"" + item.Id + "\",\"" + item.Student + "\",\"" + item.DayOfWeek + "\",\"" + string.Format("{0:HH:mm} - {1:HH:mm}", item.StartTime, item.EndTime) + "\")'>Delete</a></td>";
                content += "</tr>";
            }

            return Json(new { body = content }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> StudentIndex(string tutorid, int? dow)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var list = string.IsNullOrEmpty(tutorid)
                    ? await (from tss in db.TutorStudentSchedules
                             join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                             join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                             join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                             select new TutorStudentSchedulesViewModels
                             {
                                 Id = tss.Id,
                                 Tutor = t.Firstname + " " + t.Middlename + " " + t.Lastname,
                                 Student = s.Firstname + " " + s.Middlename + " " + s.Lastname,
                                 DayOfWeek = tss.DayOfWeek,
                                 StartTime = tss.StartTime,
                                 EndTime = tss.EndTime,
                                 Invoice = sii.Description,
                                 RemainingHours = (decimal)sii.SessionHours_Remaining,
                                 IsActive = tss.IsActive,
                                 Notes = tss.Notes
                             }).ToListAsync()
                    : await (from tss in db.TutorStudentSchedules
                             join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                             join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                             join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                             where tss.Tutor_UserAccounts_Id == tutorid && (int)tss.DayOfWeek == dow
                             select new TutorStudentSchedulesViewModels
                             {
                                 Id = tss.Id,
                                 Tutor = t.Firstname + " " + t.Middlename + " " + t.Lastname,
                                 Student = s.Firstname + " " + s.Middlename + " " + s.Lastname,
                                 DayOfWeek = tss.DayOfWeek,
                                 StartTime = tss.StartTime,
                                 EndTime = tss.EndTime,
                                 Invoice = sii.Description,
                                 RemainingHours = (decimal)sii.SessionHours_Remaining,
                                 IsActive = tss.IsActive,
                                 Notes = tss.Notes
                             }).ToListAsync();

                //ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public async Task<ActionResult> StudentCreate(string id, string dow, string start, string end, string tutorid)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var student = await db.User.FindAsync(id);
                    ViewBag.Error = "init";
                    ViewBag.StudentId = student == null ? "" : student.Id;
                    ViewBag.StudentName = student == null ? "" : student.Firstname + " " + student.Middlename + " " + student.Lastname;
                    ViewBag.StartTime = string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 8, 0, 0));
                    ViewBag.EndTime = string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 12, 0, 0));
                }

                if (!string.IsNullOrEmpty(tutorid))
                {
                    ViewBag.Error = "tutor";
                    var tutor = await db.User.FindAsync(tutorid);
                    ViewBag.TutorId = tutor == null ? "" : tutor.Id;
                    ViewBag.TutorName = tutor == null ? "" : tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
                }

                ViewBag.DOW = string.IsNullOrEmpty(dow) ? "0" : dow;
                ViewBag.StartTime = string.IsNullOrEmpty(start) ? string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 8, 0, 0)) : start.Replace("_", ":");
                ViewBag.EndTime = string.IsNullOrEmpty(end) ? string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 12, 0, 0)) : end.Replace("_", ":");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentCreate([Bind(Include = "Id,Student_UserAccounts_Id,Tutor_UserAccounts_Id,DayOfWeek,StartTime,EndTime,InvoiceItems_Id,IsActive,Notes")] TutorStudentSchedulesModels tutorSchedulesModels)
        {
            var start = new DateTime(1970, 01, 01, tutorSchedulesModels.StartTime.Hour, tutorSchedulesModels.StartTime.Minute, 0);
            var end = new DateTime(1970, 01, 01, tutorSchedulesModels.EndTime.Hour, tutorSchedulesModels.EndTime.Minute, 0);

            var isExist = await db.TutorStudentSchedules
                .Where(x => x.Student_UserAccounts_Id == tutorSchedulesModels.Student_UserAccounts_Id
                    && x.DayOfWeek == tutorSchedulesModels.DayOfWeek
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();
            if (isExist != null)
            {
                var student = await db.User.FindAsync(tutorSchedulesModels.Student_UserAccounts_Id);
                string student_name = student.Firstname + " " + student.Middlename + " " + student.Lastname;
                var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                string tutor_name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
                ModelState.AddModelError("Exist", "This schedule already exist ( " + string.Format("{0}: [Tutor: {1}] {2}, {3:HH:mm} - {4:HH:mm}", student_name, tutor_name, tutorSchedulesModels.DayOfWeek, start, end) + " ).");
            }

            if (tutorSchedulesModels.StartTime.Hour > tutorSchedulesModels.EndTime.Hour
                || (tutorSchedulesModels.StartTime.Hour == tutorSchedulesModels.EndTime.Hour
                    && tutorSchedulesModels.StartTime.Minute > tutorSchedulesModels.EndTime.Minute))
            {
                ModelState.AddModelError("Schedule", "The Start Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.StartTime) + " ) field cannot greater than End Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.EndTime) + " ) field.");
            }

            if (tutorSchedulesModels.InvoiceItems_Id == Guid.Empty)
            {
                ModelState.AddModelError("InvoiceItems_Id", "The field Invoice is required.");
            }

            if (ModelState.IsValid)
            {
                tutorSchedulesModels.Id = Guid.NewGuid();
                tutorSchedulesModels.StartTime = start;
                tutorSchedulesModels.EndTime = end;
                tutorSchedulesModels.IsActive = true;
                db.TutorStudentSchedules.Add(tutorSchedulesModels);
                await db.SaveChangesAsync();

                return RedirectToAction("StudentCreate", new { id = tutorSchedulesModels.Student_UserAccounts_Id, dow = (int)tutorSchedulesModels.DayOfWeek, start = string.Format("{0:HH_mm}", start), end = string.Format("{0:HH_mm}", end) });
                //return RedirectToAction("StudentIndex");
            }

            return View(tutorSchedulesModels);
        }

        public async Task<ActionResult> StudentEdit(Guid? id)
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
                TutorStudentSchedulesModels tutorStudentSchedulesModels = await db.TutorStudentSchedules.FindAsync(id);
                if (tutorStudentSchedulesModels == null)
                {
                    return HttpNotFound();
                }

                var tutor = await db.User.FindAsync(tutorStudentSchedulesModels.Tutor_UserAccounts_Id);
                ViewBag.TutorName = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;

                var student = await db.User.FindAsync(tutorStudentSchedulesModels.Student_UserAccounts_Id);
                ViewBag.StudentName = student.Firstname + " " + student.Middlename + " " + student.Lastname;

                List<object> newList = new List<object>();
                var data = (from si in db.SaleInvoices
                            join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                            where si.Due == 0 && si.Customer_UserAccounts_Id == tutorStudentSchedulesModels.Student_UserAccounts_Id && sii.SessionHours_Remaining > 0
                            orderby sii.Description ascending
                            select new { sii }).ToList();
                foreach (var item in data)
                {
                    newList.Add(new
                    {
                        Id = item.sii.Id,
                        Name = item.sii.Description + " [Qty: " + item.sii.Qty + ", Avail. Hours: " + item.sii.SessionHours_Remaining + " hrs]"
                    });
                }
                ViewBag.listLesson = new SelectList(newList, "Id", "Name");

                return View(tutorStudentSchedulesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentEdit([Bind(Include = "Id,Student_UserAccounts_Id,Tutor_UserAccounts_Id,DayOfWeek,StartTime,EndTime,InvoiceItems_Id,IsActive,Notes")] TutorStudentSchedulesModels tutorSchedulesModels)
        {
            var start = new DateTime(1970, 01, 01, tutorSchedulesModels.StartTime.Hour, tutorSchedulesModels.StartTime.Minute, 0);
            var end = new DateTime(1970, 01, 01, tutorSchedulesModels.EndTime.Hour, tutorSchedulesModels.EndTime.Minute, 0);

            var isExist = await db.TutorStudentSchedules
                .Where(x => x.Id != tutorSchedulesModels.Id 
                    && x.Student_UserAccounts_Id == tutorSchedulesModels.Student_UserAccounts_Id
                    && x.DayOfWeek == tutorSchedulesModels.DayOfWeek
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();
            if (isExist != null)
            {
                var _student = await db.User.FindAsync(tutorSchedulesModels.Student_UserAccounts_Id);
                string student_name = _student.Firstname + " " + _student.Middlename + " " + _student.Lastname;
                var _tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                string tutor_name = _tutor.Firstname + " " + _tutor.Middlename + " " + _tutor.Lastname;
                ModelState.AddModelError("Exist", "This schedule already exist ( " + string.Format("{0}: [Tutor: {1}] {2}, {3:HH:mm} - {4:HH:mm}", student_name, tutor_name, tutorSchedulesModels.DayOfWeek, start, end) + " ).");
            }

            if (tutorSchedulesModels.StartTime.Hour > tutorSchedulesModels.EndTime.Hour
                || (tutorSchedulesModels.StartTime.Hour == tutorSchedulesModels.EndTime.Hour
                    && tutorSchedulesModels.StartTime.Minute > tutorSchedulesModels.EndTime.Minute))
            {
                ModelState.AddModelError("Schedule", "The Start Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.StartTime) + " ) field cannot greater than End Time ( " + string.Format("{0:HH:mm}", tutorSchedulesModels.EndTime) + " ) field.");
            }

            if (tutorSchedulesModels.InvoiceItems_Id == Guid.Empty)
            {
                ModelState.AddModelError("InvoiceItems_Id", "The field Invoice is required.");
            }

            if (ModelState.IsValid)
            {
                var current_data = await db.TutorStudentSchedules.FindAsync(tutorSchedulesModels.Id);
                current_data.Tutor_UserAccounts_Id = tutorSchedulesModels.Tutor_UserAccounts_Id;
                current_data.Student_UserAccounts_Id = tutorSchedulesModels.Student_UserAccounts_Id;
                current_data.DayOfWeek = tutorSchedulesModels.DayOfWeek;
                current_data.StartTime = start;
                current_data.EndTime = end;
                current_data.InvoiceItems_Id = tutorSchedulesModels.InvoiceItems_Id;
                current_data.IsActive = tutorSchedulesModels.IsActive;
                current_data.Notes = tutorSchedulesModels.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("StudentIndex");
            }

            var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
            ViewBag.TutorName = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;

            var student = await db.User.FindAsync(tutorSchedulesModels.Student_UserAccounts_Id);
            ViewBag.StudentName = student.Firstname + " " + student.Middlename + " " + student.Lastname;

            List<object> newList = new List<object>();
            var data = (from si in db.SaleInvoices
                        join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                        where si.Due == 0 && si.Customer_UserAccounts_Id == tutorSchedulesModels.Student_UserAccounts_Id && sii.SessionHours_Remaining > 0
                        orderby sii.Description ascending
                        select new { sii }).ToList();
            foreach (var item in data)
            {
                newList.Add(new
                {
                    Id = item.sii.Id,
                    Name = item.sii.Description + " [Qty: " + item.sii.Qty + ", Avail. Hours: " + item.sii.SessionHours_Remaining + " hrs]"
                });
            }
            ViewBag.listLesson = new SelectList(newList, "Id", "Name");

            return View(tutorSchedulesModels);
        }
        #endregion

        public async Task<JsonResult> DeleteSchedule(Guid id, string key)
        {
            if (key == "tutor")
            {
                var schedule = await db.TutorSchedules.FindAsync(id);
                db.TutorSchedules.Remove(schedule);
            }
            else
            {
                var schedule = await db.TutorStudentSchedules.FindAsync(id);
                db.TutorStudentSchedules.Remove(schedule);
            }

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }

        private void AddHeaderTable(DateTime timeStart, DateTime timeEnd, List<string> list)
        {
            if (timeStart <= timeEnd)
            {
                list.Add(string.Format("{0:HH:mm}", timeStart));
                AddHeaderTable(timeStart.AddMinutes(30), timeEnd, list);
            }
        }

        private void AddBookedTime(DateTime timeStart,DateTime timeEnd, List<string> list)
        {
            if (timeStart <= timeEnd)
            {
                list.Add(string.Format("{0:HH:mm}", timeStart));
                AddBookedTime(timeStart.AddMinutes(30), timeEnd, list);
            }
        }

        public async Task<JsonResult> GetAvailableSchedule(string tutor_id, string language_id, DayOfWeekEnum dow, DateTime start, DateTime end)
        {
            string table_html = "";
            table_html += @"
                <table class='table table-striped table-condensed'>
                    <thead>
                        <tr>
                            <th>Tutor</th>";

            DateTime startFilter = new DateTime(1970, 1, 1, start.Hour, start.Minute, 0);
            DateTime endFilter = new DateTime(1970, 1, 1, end.Hour, end.Minute, 0);
            
            List<string> list_header = new List<string>();
            AddHeaderTable(startFilter, endFilter, list_header);
            foreach (var header in list_header)
            {
                table_html += "<th>" + header + "</th>";
            }
            
            table_html += @"
                        </tr>
                    </thead>
                </thead>";

            var items = string.IsNullOrEmpty(tutor_id)
                ? await (from u in db.User
                         join ts in db.TutorSchedules on u.Id equals ts.Tutor_UserAccounts_Id
                         where ts.DayOfWeek == dow && ts.IsActive == true && (u.Interest != null || u.Interest != "")
                         orderby u.Firstname ascending
                         select new { u, ts }).ToListAsync()
                : await (from u in db.User
                         join ts in db.TutorSchedules on u.Id equals ts.Tutor_UserAccounts_Id
                         where ts.Tutor_UserAccounts_Id == tutor_id && ts.DayOfWeek == dow && ts.IsActive == true && (u.Interest != null || u.Interest != "")
                         orderby u.Firstname ascending
                         select new { u, ts }).ToListAsync();
            table_html += "<tbody>";
            
            foreach (var item in items)
            {
                bool isLanguage = false;
                var languages = JsonConvert.DeserializeObject<List<InterestViewModels>>(item.u.Interest);
                foreach (var language in languages)
                {
                    if (language_id == language.Languages_Id) { isLanguage = true; }
                }

                if (isLanguage)
                {
                    List<string> list_booked = new List<string>();
                    List<string> list_expired = new List<string>();
                    var bookings = await db.TutorStudentSchedules.Where(x => x.Tutor_UserAccounts_Id == item.u.Id && x.DayOfWeek == dow && x.IsActive == true).OrderBy(x => x.StartTime).ToListAsync();
                    foreach (var booking in bookings)
                    {
                        decimal hours_remaining = db.SaleInvoiceItems.Find(booking.InvoiceItems_Id).SessionHours_Remaining.Value;
                        DateTime book_start = booking.StartTime;
                        DateTime book_end = booking.EndTime;
                        if (hours_remaining > 0) { AddBookedTime(book_start, book_end, list_booked); }
                        else { AddBookedTime(book_start, book_end, list_expired); }
                    }

                    table_html += "<tr><td>" + string.Format("{0}", item.u.Firstname) + "</td>";
                    foreach (var t in list_header)
                    {
                        if (list_booked.Contains(t))
                        {
                            table_html += "<td><a target='_blank' href='" + Url.Action("StudentIndex", "Schedules", new { dow = (int)dow, tutorid = item.u.Id }) + "'><span class='badge badge-danger d-block'>Booked</span></a></td>";
                        }
                        else if (list_expired.Contains(t))
                        {
                            table_html += "<td><a target='_blank' href='" + Url.Action("StudentIndex", "Schedules", new { dow = (int)dow, tutorid = item.u.Id }) + "'><span class='badge bg-orange d-block'>Expired</span></a></td>";
                        }
                        else
                        {
                            string[] splitTime = t.Split(':');
                            DateTime timeHeader = new DateTime(1970, 1, 1, int.Parse(splitTime[0]), int.Parse(splitTime[1]), 0);
                            if (timeHeader >= item.ts.StartTime && timeHeader <= item.ts.EndTime)
                            {
                                table_html += "<td><a target='_blank' href='" + Url.Action("StudentCreate", "Schedules", new { dow = (int)dow, start = t.Replace(":", "_"), tutorid = item.u.Id }) + "'><span class='badge badge-success d-block'>Free</span></a></td>";
                            }
                            else
                            {
                                table_html += "<td></td>";
                            }
                        }
                    }
                    table_html += "</tr>";
                }
            }

            table_html += "</tbody></table>";

            return Json(new { content = table_html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }
        }
    }
}