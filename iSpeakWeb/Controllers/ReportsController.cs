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
    public class ReportsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        #region GetNewStudent
        public async Task<JsonResult> GetNewStudent(string branch_id, int month, int year)
        {
            DateTime dateFrom = new DateTime(year, month, 1, 0, 0, 0);
            DateTime dateTo = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            List<NewStudentViewModels> list = await db.Database.SqlQuery<NewStudentViewModels>(@"
                SELECT
                u.Firstname+' '+u.Middlename+' '+u.Lastname Name,inv.qty_lesson Qty
                FROM AspNetUsers u
                INNER JOIN (
	                SELECT s.Customer_UserAccounts_Id student_id,COUNT(si.Id) qty_lesson 
	                FROM SaleInvoices s
	                INNER JOIN SaleInvoiceItems si ON s.Id=si.SaleInvoices_Id
		                WHERE s.Branches_Id='" + branch_id + @"'  
			                AND s.Cancelled=0
			                AND s.Due=0
                            AND si.LessonPackages_Id IS NOT NULL
                            AND s.Timestamp BETWEEN '" + dateFrom.ToString("yyyy-MM-dd") + "' AND '" + dateTo.ToString("yyyy-MM-dd 23:59:59.000") + @"'
	                GROUP BY s.Customer_UserAccounts_Id
                ) inv ON u.Id=inv.student_id
            ").ToListAsync();

            return Json(new { obj = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetTutorPayroll
        public async Task<JsonResult> GetTutorPayroll(Guid branch_id, int month, int year)
        {
            DateTime dateFrom = new DateTime(year, month, 1, 0, 0, 0);
            DateTime dateTo = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            List<TutorPayrollViewModels> list = new List<TutorPayrollViewModels>();

            var tutors = await (from u in db.User
                                join ur in db.UserRole on u.Id equals ur.UserId
                                join r in db.Role on ur.RoleId equals r.Id
                                where r.Name.ToLower() == "tutor"
                                orderby u.Firstname
                                select new { u }).ToListAsync();

            foreach (var tutor in tutors)
            {
                var sessions = await (from ls in db.LessonSessions
                                      join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                                      join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                                      where si.Branches_Id == branch_id 
                                        && ls.Tutor_UserAccounts_Id == tutor.u.Id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                                      select new { ls }).ToListAsync();
                if (sessions.Count > 0)
                {
                    decimal tot_payable = 0;
                    foreach (var s in sessions)
                    {
                        tot_payable += s.ls.SessionHours * s.ls.HourlyRates_Rate;
                    }

                    list.Add(new TutorPayrollViewModels
                    {
                        TutorId = tutor.u.Id,
                        Name = tutor.u.Firstname + " " + tutor.u.Middlename + " " + tutor.u.Lastname,
                        TotalHours = sessions.Sum(x => x.ls.SessionHours),
                        TotalPayable = tot_payable,
                        Details = "<a href='#' onclick='Details(\"" + branch_id.ToString() + "\",\"" + month + "\",\"" + year + "\",\"" + tutor.u.Id + "\")'>Details</a>"
                    });
                }
            }

            return Json(new { obj = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetDetails
        public async Task<JsonResult> GetDetails(Guid branch_id, int month, int year, string tutor_id)
        {
            DateTime dateFrom = new DateTime(year, month, 1, 0, 0, 0);
            DateTime dateTo = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            var result = await (from ls in db.LessonSessions
                                join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
                                join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                                join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                                where si.Branches_Id == branch_id 
                                    && ls.Tutor_UserAccounts_Id == tutor_id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                                orderby ls.Timestamp
                                select new { ls, u, sii }).ToListAsync();

            string tutor_name = "";
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Description</th>
                                                <th>Session Hours</th>
                                                <th>Hourly Rate</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in result)
            {
                tutor_name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname;
                message += @"<tr>
                                <td>" + string.Format("{0:yyyy/MM/dd HH:mm}", item.ls.Timestamp) + @"</td>
                                <td>" + item.sii.Description + @"</td>
                                <td>" + string.Format("{0:N2}", item.ls.SessionHours) + @"</td>
                                <td>" + string.Format("{0:N2}", item.ls.HourlyRates_Rate) + @"</td>
                            </tr>";
            }
            
            return Json(new { tutor_name, obj = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult NewStudent()
        {
            return View();
        }

        public ActionResult Payroll()
        {
            return View();
        }
    }
}