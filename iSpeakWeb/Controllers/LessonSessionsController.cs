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
    public class LessonSessionsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        #region JSON
        #region Get Lesson List
        public JsonResult GetLessonList(string student_id)
        {
            List<object> newList = new List<object>();
            var data = (from si in db.SaleInvoices
                        join sii in db.SaleInvoiceItems on si.Id equals sii.SaleInvoices_Id
                        where si.Customer_UserAccounts_Id == student_id && sii.SessionHours > 0
                        orderby sii.Description ascending
                        select new { sii }).ToList();
            foreach (var item in data)
            {
                newList.Add(new
                {
                    Id = item.sii.Id,
                    Name = item.sii.Description
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
                decimal remaining = db.SaleInvoiceItems.Where(x => x.Id == item.sale_invoice_item_id).FirstOrDefault().SessionHours.Value;
                if (data.sii.SessionHours.Value < hour)
                {
                    isValid = false;
                    message = "INVALID! " + data.u.Firstname + " " + data.u.Middlename + " " + data.u.Lastname + " - " + data.sii.Description + " remaining hours is less than " + hour + " hr.";
                    break;
                }
            }

            return Json(new { isValid, message }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        public async Task<ActionResult> Index()
        {
            var data = await (from ls in db.LessonSessions
                              join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
                              join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                              join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                              select new LessonSessionsViewModels
                              {
                                  Id = ls.Id,
                                  Timestamp = ls.Timestamp,
                                  Lesson = lp.Name,
                                  Tutor = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                                  SessionHours = ls.SessionHours,
                                  HourlyRates_Rate = ls.HourlyRates_Rate,
                                  TravelCost = ls.TravelCost,
                                  TutorTravelCost = ls.TutorTravelCost,
                                  Deleted = ls.Deleted
                              }).ToListAsync();
            return View(data);
        }

        public ActionResult Create()
        {
            var users = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where r.Name == "Tutor" || r.Name == "Student"
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

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Timestamp,Tutor_UserAccounts_Id,SessionHours,Review,InternalNotes")] LessonSessionsModels lessonSessionsModels, string Items)
        {
            if (ModelState.IsValid)
            {
                List<LessonSessionsDetails> details = JsonConvert.DeserializeObject<List<LessonSessionsDetails>>(Items);
                foreach(var item in details)
                {
                    var sale_invoice_item = db.SaleInvoiceItems.Where(x => x.Id == item.sale_invoice_item_id).FirstOrDefault();

                    LessonSessionsModels model = new LessonSessionsModels();
                    model.Id = Guid.NewGuid();
                    model.Timestamp = new DateTime(lessonSessionsModels.Timestamp.Year, lessonSessionsModels.Timestamp.Month, lessonSessionsModels.Timestamp.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    model.SaleInvoiceItems_Id = item.sale_invoice_item_id;
                    model.SessionHours = lessonSessionsModels.SessionHours;
                    model.Review = lessonSessionsModels.Review;
                    model.InternalNotes = lessonSessionsModels.InternalNotes;
                    model.Deleted = false;
                    model.Tutor_UserAccounts_Id = lessonSessionsModels.Tutor_UserAccounts_Id;
                    var hourly_rate = db.HourlyRates.Where(x => x.UserAccounts_Id == lessonSessionsModels.Tutor_UserAccounts_Id).ToList();
                    if (hourly_rate.Count == 0)
                    {
                        model.HourlyRates_Rate = 0; //this tutor not found in hourly rate
                    }
                    else
                    {
                        foreach (var subitem in hourly_rate)
                        {
                            model.HourlyRates_Rate = subitem.Rate; //use tutor rate with null package
                            if (subitem.LessonPackages_Id == sale_invoice_item.LessonPackages_Id.Value) { model.HourlyRates_Rate = subitem.Rate; break; } //found tutor with exact package
                        }
                    }
                    model.TravelCost = sale_invoice_item.TravelCost / (int)Math.Ceiling(sale_invoice_item.SessionHours.Value * lessonSessionsModels.SessionHours);
                    model.TutorTravelCost = sale_invoice_item.TutorTravelCost / (int)Math.Ceiling(sale_invoice_item.SessionHours.Value * lessonSessionsModels.SessionHours);
                    model.Adjustment = 0;
                    db.LessonSessions.Add(model);

                    sale_invoice_item.SessionHours = sale_invoice_item.SessionHours.Value - lessonSessionsModels.SessionHours;
                    db.Entry(sale_invoice_item).State = EntityState.Modified;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            var users = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where r.Name == "Tutor" || r.Name == "Student"
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

            return View(lessonSessionsModels);
        }
    }
}