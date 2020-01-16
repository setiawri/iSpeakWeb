using iSpeak.Common;
using iSpeak.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class LessonSessionsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region JSON
        #region GetData
        public async Task<JsonResult> GetData()
        {
            Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
            var is_student = await (from u in db.User
                                    join ur in db.UserRole on u.Id equals ur.UserId
                                    join r in db.Role on ur.RoleId equals r.Id
                                    where r.Name.ToLower() == "student" && u.UserName == User.Identity.Name
                                    select new { u }).FirstOrDefaultAsync();

            List<LessonSessionsViewModels> list = new List<LessonSessionsViewModels>();

            var sessions = (is_student == null)
                ? await (from ls in db.LessonSessions
                         join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
                         join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                         join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                         join s in db.User on si.Customer_UserAccounts_Id equals s.Id
                         join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                         where ls.Branches_Id == user_branch
                         select new { ls, u, sii, si, s, lp }).ToListAsync()
                : await (from ls in db.LessonSessions
                         join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
                         join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                         join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                         join s in db.User on si.Customer_UserAccounts_Id equals s.Id
                         join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                         where ls.Branches_Id == user_branch && s.UserName == User.Identity.Name
                         select new { ls, u, sii, si, s, lp }).ToListAsync();

            foreach (var session in sessions)
            {
                list.Add(new LessonSessionsViewModels
                {
                    Id = session.ls.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", session.ls.Timestamp),
                    Lesson = session.lp.Name,
                    Student = session.s.Firstname + " " + session.s.Middlename + " " + session.s.Lastname,
                    Tutor = session.u.Firstname + " " + session.u.Middlename + " " + session.u.Lastname,
                    SessionHours = session.ls.SessionHours,
                    HourlyRates_Rate = session.ls.HourlyRates_Rate,
                    TravelCost = session.ls.TravelCost,
                    TutorTravelCost = session.ls.TutorTravelCost,
                    Deleted = session.ls.Deleted,
                    Notes_Cancel = session.ls.Notes_Cancel
                });
            }

            int totalRows = list.Count;

            //Server Side Parameters
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            //Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list
                    .Where(x => x.Timestamp.ToLower().Contains(searchValue.ToLower())
                        || x.Lesson.ToLower().Contains(searchValue.ToLower())
                        || x.Student.ToLower().Contains(searchValue.ToLower())
                        || x.Tutor.ToLower().Contains(searchValue.ToLower())
                    ).ToList();
            }
            int totalRowsFiltered = list.Count;

            //Sorting
            list = list.OrderBy(sortColumnName + " " + sortDirection).ToList();

            //Paging
            list = list.Skip(start).Take(length).ToList();

            return Json(new { data = list, draw = Request["draw"], recordsTotal = totalRows, recordsFiltered = totalRowsFiltered }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Lesson List
        public JsonResult GetLessonList(string student_id)
        {
            List<object> newList = new List<object>();
            var data = (from si in db.SaleInvoices
                        join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                        where si.Due == 0 && si.Customer_UserAccounts_Id == student_id && sii.SessionHours_Remaining > 0
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
            var ddl = new SelectList(newList, "Id", "Name");

            return Json(new { ddl }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Tutor List
        public JsonResult GetTutorList(Guid sale_invoice_item_id)
        {
            List<object> newList = new List<object>();
            Guid lesson_pck_id = db.SaleInvoiceItems.Where(x => x.Id == sale_invoice_item_id).FirstOrDefault().LessonPackages_Id.Value;
            var tutors = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where r.Name == "Tutor"
                         orderby u.Firstname
                         select new { u, r }).ToList();
            foreach (var tutor in tutors)
            {
                var tutor_rate = db.HourlyRates.Where(x => x.UserAccounts_Id == tutor.u.Id && x.LessonPackages_Id == lesson_pck_id).FirstOrDefault();
                if (tutor_rate != null)
                {
                    newList.Add(new
                    {
                        Id = tutor.u.Id,
                        Name = tutor.u.Firstname + " " + tutor.u.Middlename + " " + tutor.u.Lastname
                    });
                }
                else
                {
                    var tutor_rate_all = db.HourlyRates.Where(x => x.UserAccounts_Id == tutor.u.Id && x.LessonPackages_Id == null).FirstOrDefault();
                    if (tutor_rate_all != null)
                    {
                        newList.Add(new
                        {
                            Id = tutor.u.Id,
                            Name = tutor.u.Firstname + " " + tutor.u.Middlename + " " + tutor.u.Lastname
                        });
                    }
                }
            }
            var ddl = new SelectList(newList, "Id", "Name");

            return Json(new { ddl }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Check Remaining Hour
        public JsonResult CheckRemainingHour(string items, decimal hour)
        {
            bool isValid = true; string message = "";
            List<LessonSessionsDetails> list = JsonConvert.DeserializeObject<List<LessonSessionsDetails>>(items);
            foreach (var item in list)
            {
                var data = (from si in db.SaleInvoices
                            join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            where sii.Id == item.sale_invoice_item_id
                            select new { si, sii, u }).FirstOrDefault();
                decimal remaining = db.SaleInvoiceItems.Where(x => x.Id == item.sale_invoice_item_id).FirstOrDefault().SessionHours_Remaining.Value;
                if (data.sii.SessionHours_Remaining.Value < hour)
                {
                    isValid = false;
                    message = "INVALID! " + data.u.Firstname + " " + data.u.Middlename + " " + data.u.Lastname + " - " + data.sii.Description + " remaining hours is less than " + hour + " hr.";
                    break;
                }
            }

            return Json(new { isValid, message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Get Session Data Index
        public JsonResult GetSession(Guid id, string lesson, string student, string tutor)
        {
            var data = db.LessonSessions.Where(x => x.Id == id).FirstOrDefault();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Lesson</th>
                                                <th>Student</th>
                                                <th>Tutor</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            message += @"<tr>
                            <td>" + string.Format("{0:yyyy/MM/dd HH:mm}", data.Timestamp) + @"</td>
                            <td>" + lesson + @"</td>
                            <td>" + student + @"</td>
                            <td>" + tutor + @"</td>
                        </tr>";
            message += "</tbody></table></div><br />";
            message += @"<div class='row'>
                            <div class='col-md-6'>
                                <div class='form-group'>
                                    <label>Review</label>
                                    <textarea class='form-control' rows='3'>" + data.Review + @"</textarea>
                                </div>
                            </div>";
            
            Permission p = new Permission();
            bool isShowInternalNotes = p.IsGranted(User.Identity.Name, "lessonsessions_showinternalnotes");
            if (isShowInternalNotes)
            {
                message += @"<div class='col-md-6'>
                                <div class='form-group'>
                                    <label>Internal Notes</label>
                                    <textarea class='form-control' rows='3'>" + data.InternalNotes + @"</textarea>
                                </div>
                            </div>";
            }
            message += "</div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Sessions
        public async Task<JsonResult> Cancelled(Guid id, string notes)
        {
            var lesson_session = await db.LessonSessions.FindAsync(id);
            int total_session_before_cancel = db.LessonSessions.Where(x => x.PayrollPaymentItems_Id == lesson_session.PayrollPaymentItems_Id).ToList().Count;

            if (total_session_before_cancel == 1)
            {
                var payroll_pay_items = await db.PayrollPaymentItems.FindAsync(lesson_session.PayrollPaymentItems_Id);
                db.PayrollPaymentItems.Remove(payroll_pay_items);
            }

            lesson_session.Deleted = true;
            lesson_session.Notes_Cancel = notes;
            lesson_session.PayrollPaymentItems_Id = null;
            db.Entry(lesson_session).State = EntityState.Modified;

            var sale_inv_item = await db.SaleInvoiceItems.Where(x => x.Id == lesson_session.SaleInvoiceItems_Id).FirstOrDefaultAsync();
            sale_inv_item.SessionHours_Remaining += lesson_session.SessionHours;
            db.Entry(sale_inv_item).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.TanggalNow = string.Format("{0:yyyy/MM/dd HH:mm}", DateTime.Now);
                ViewBag.TanggalUtc = string.Format("{0:yyyy/MM/dd HH:mm}", DateTime.UtcNow);

                ViewBag.IsShowHourlyRate = p.IsGranted(User.Identity.Name, "lessonsessions_showhourlyrate");
                ViewBag.Cancel = p.IsGranted(User.Identity.Name, "lessonsessions_cancel");
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                return View();
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
                             where u.Active == true && r.Name == "Tutor" || r.Name == "Student"
                             orderby u.Firstname
                             select new { u, r }).ToList();
                List<object> tutor_list = new List<object>();
                List<object> student_list = new List<object>();
                foreach (var item in users)
                {
                    if (item.r.Name == "Tutor")
                    {
                        tutor_list.Add(new
                        {
                            Id = item.u.Id,
                            Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                        });
                    }
                    else
                    {
                        student_list.Add(new
                        {
                            Id = item.u.Id,
                            Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                        });
                    }
                }
                ViewBag.listTutor = new SelectList(tutor_list, "Id", "Name");
                ViewBag.listStudent = new SelectList(student_list, "Id", "Name");
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Branches_Id,Timestamp,Tutor_UserAccounts_Id,SessionHours,Review,InternalNotes")] LessonSessionsModels lessonSessionsModels, string Items, string Description, bool ChangeSchedule, bool Waive)
        {
            if (ModelState.IsValid)
            {
                var hourly_rate = db.HourlyRates.Where(x => x.UserAccounts_Id == lessonSessionsModels.Tutor_UserAccounts_Id).ToList();
                Guid ppi_id = Guid.NewGuid();
                if (ChangeSchedule)
                {
                    lessonSessionsModels.SessionHours = 0; //Change Schedule checked, set hours = 0
                }
                else
                {
                    #region Payroll Payment Items Add
                    PayrollPaymentItemsModels payrollPaymentItemsModels = new PayrollPaymentItemsModels
                    {
                        Id = ppi_id,
                        Timestamp = lessonSessionsModels.Timestamp,
                        Description = Description,
                        Hour = lessonSessionsModels.SessionHours,
                        UserAccounts_Id = lessonSessionsModels.Tutor_UserAccounts_Id
                    };

                    if (hourly_rate.Count == 0)
                    {
                        payrollPaymentItemsModels.HourlyRate = 0; //this tutor not found in hourly rate
                    }
                    else
                    {
                        foreach (var rate in hourly_rate)
                        {
                            payrollPaymentItemsModels.HourlyRate = rate.Rate; //use tutor rate with null branch
                            if (lessonSessionsModels.Branches_Id.HasValue && rate.Branches_Id == lessonSessionsModels.Branches_Id.Value) //found tutor with exact branch
                            {
                                payrollPaymentItemsModels.HourlyRate = rate.Rate;
                                break;
                            }
                        }
                    }

                    if (Waive) { payrollPaymentItemsModels.Hour = 0; } //if Waives Tutor Fee checked, Payment Hour set to 0

                    payrollPaymentItemsModels.Amount = payrollPaymentItemsModels.Hour * payrollPaymentItemsModels.HourlyRate;
                    db.PayrollPaymentItems.Add(payrollPaymentItemsModels);
                    #endregion
                }
                #region Lesson Sessions Add
                List<LessonSessionsDetails> details = JsonConvert.DeserializeObject<List<LessonSessionsDetails>>(Items);
                foreach (var item in details)
                {
                    var sale_invoice_item = db.SaleInvoiceItems.Where(x => x.Id == item.sale_invoice_item_id).FirstOrDefault();

                    LessonSessionsModels model = new LessonSessionsModels
                    {
                        Id = Guid.NewGuid(),
                        Branches_Id = lessonSessionsModels.Branches_Id.Value,
                        Timestamp = lessonSessionsModels.Timestamp, //TimeZoneInfo.ConvertTimeToUtc(lessonSessionsModels.Timestamp),
                        SaleInvoiceItems_Id = item.sale_invoice_item_id,
                        SessionHours = lessonSessionsModels.SessionHours,
                        Review = item.review, //lessonSessionsModels.Review;
                        InternalNotes = item.internal_notes, //lessonSessionsModels.InternalNotes;
                        Deleted = false,
                        Tutor_UserAccounts_Id = lessonSessionsModels.Tutor_UserAccounts_Id,
                        TravelCost = ChangeSchedule ? 0 : sale_invoice_item.TravelCost / (int)Math.Ceiling(sale_invoice_item.SessionHours.Value * lessonSessionsModels.SessionHours),
                        TutorTravelCost = ChangeSchedule ? 0 : sale_invoice_item.TutorTravelCost / (int)Math.Ceiling(sale_invoice_item.SessionHours.Value * lessonSessionsModels.SessionHours),
                        Adjustment = 0,
                        PayrollPaymentItems_Id = ChangeSchedule ? (Guid?)null : ppi_id
                    };

                    if (hourly_rate.Count == 0)
                    {
                        model.HourlyRates_Rate = 0; //this tutor not found in hourly rate
                    }
                    else
                    {
                        foreach (var subitem in hourly_rate)
                        {
                            model.HourlyRates_Rate = subitem.Rate / details.Count; //use tutor rate with null package
                            if (subitem.LessonPackages_Id == sale_invoice_item.LessonPackages_Id.Value) //found tutor with exact package
                            {
                                model.HourlyRates_Rate = subitem.Rate / details.Count;
                                break;
                            }
                        }
                    }
                    db.LessonSessions.Add(model);

                    sale_invoice_item.SessionHours_Remaining = sale_invoice_item.SessionHours_Remaining.Value - lessonSessionsModels.SessionHours;
                    db.Entry(sale_invoice_item).State = EntityState.Modified;
                }
                #endregion

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var users = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where u.Active == true && r.Name == "Tutor" || r.Name == "Student"
                         orderby u.Firstname
                         select new { u, r }).ToList();
            List<object> tutor_list = new List<object>();
            List<object> student_list = new List<object>();
            foreach (var item in users)
            {
                if (item.r.Name == "Tutor")
                {
                    tutor_list.Add(new
                    {
                        Id = item.u.Id,
                        Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                    });
                }
                else
                {
                    student_list.Add(new
                    {
                        Id = item.u.Id,
                        Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                    });
                }
            }
            ViewBag.listTutor = new SelectList(tutor_list, "Id", "Name");
            ViewBag.listStudent = new SelectList(student_list, "Id", "Name");
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

            return View(lessonSessionsModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                LessonSessionsModels lessonSessionsModels = await db.LessonSessions.Where(x => x.Id == id).FirstOrDefaultAsync();
                return View(lessonSessionsModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Branches_Id,Timestamp,SaleInvoiceItems_Id,SessionHours,Review,InternalNotes,Deleted,Tutor_UserAccounts_Id,HourlyRates_Rate,TravelCost,TutorTravelCost,Adjustment,PayrollPaymentItems_Id")] LessonSessionsModels lessonSessionsModels)
        {
            if (ModelState.IsValid)
            {
                var current_data = await db.LessonSessions.FindAsync(lessonSessionsModels.Id);
                current_data.HourlyRates_Rate = lessonSessionsModels.HourlyRates_Rate;
                current_data.TravelCost = lessonSessionsModels.TravelCost;
                current_data.TutorTravelCost = lessonSessionsModels.TutorTravelCost;
                current_data.Review = lessonSessionsModels.Review;
                current_data.InternalNotes = lessonSessionsModels.InternalNotes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(lessonSessionsModels);
        }
    }
}