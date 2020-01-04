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
    public class SchedulesController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region TUTOR
        public async Task<JsonResult> GetTutorSchedule(string tutor_id)
        {
            var items = await (from ts in db.TutorSchedules
                               join u in db.User on ts.Tutor_UserAccounts_Id equals u.Id
                               where ts.Tutor_UserAccounts_Id == tutor_id
                               orderby ts.DayOfWeek
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
                content += @"<tr>
                                <td>" + item.Tutor + @"</td>
                                <td>" + item.DayOfWeek + @"</td>
                                <td>" + string.Format("{0:HH:mm} - {1:HH:mm}", item.StartTime, item.EndTime) + @"</td>
                            </tr>";
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
                var list = await (from ts in db.TutorSchedules
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
                                  }).ToListAsync();

                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public async Task<ActionResult> TutorCreate(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var tutor = await db.User.FindAsync(id);
                    ViewBag.Error = "init";
                    ViewBag.TutorId = tutor == null ? "" : tutor.Id;
                    ViewBag.TutorName = tutor == null ? "" : tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
                    ViewBag.StartTime = string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 8, 0, 0));
                    ViewBag.EndTime = string.Format("{0:HH:mm}", new DateTime(1970, 1, 1, 12, 0, 0));
                }
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

                return RedirectToAction("TutorCreate", new { id = tutorSchedulesModels.Tutor_UserAccounts_Id });
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

                var tutor = await db.User.FindAsync(tutorSchedulesModels.Tutor_UserAccounts_Id);
                ViewBag.TutorName = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;
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
        public async Task<ActionResult> StudentIndex()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var list = await (from tss in db.TutorStudentSchedules
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
                                      IsActive = tss.IsActive,
                                      Notes = tss.Notes
                                  }).ToListAsync();

                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View(list);
            }
        }

        public ActionResult StudentCreate()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
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

                return RedirectToAction("StudentIndex");
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
    }
}