using iSpeak.Common;
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
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));
            List<NewStudentViewModels> list = await db.Database.SqlQuery<NewStudentViewModels>(@"
                SELECT
                u.Firstname+' '+ISNULL(u.Middlename,'')+' '+ISNULL(u.Lastname,'') Name,inv.qty_lesson Qty
                FROM AspNetUsers u
                INNER JOIN (
	                SELECT s.Customer_UserAccounts_Id student_id,COUNT(si.Id) qty_lesson 
	                FROM SaleInvoices s
	                INNER JOIN SaleInvoiceItems si ON s.Id=si.SaleInvoices_Id
		                WHERE s.Branches_Id='" + branch_id + @"'  
			                AND s.Cancelled=0
			                AND s.Due=0
                            AND si.LessonPackages_Id IS NOT NULL
                            AND s.Timestamp BETWEEN '" + dateFrom + "' AND '" + dateTo + @"'
	                GROUP BY s.Customer_UserAccounts_Id
                ) inv ON u.Id=inv.student_id
            ").ToListAsync();

            return Json(new { obj = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetTutorPayroll
        public async Task<JsonResult> GetTutorPayroll(Guid branch_id, int month, int year)
        {
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));
            List<TutorPayrollViewModels> list = new List<TutorPayrollViewModels>();

            var tutors = await (from u in db.User
                                join ur in db.UserRole on u.Id equals ur.UserId
                                join r in db.Role on ur.RoleId equals r.Id
                                where r.Name.ToLower() == "tutor"
                                orderby u.Firstname
                                select new { u }).ToListAsync();

            foreach (var tutor in tutors)
            {
                decimal tot_hours = 0;
                decimal tot_payable = 0;
                var payrolls = (from ppi in db.PayrollPaymentItems
                                join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
                                join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                                join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                                where si.Branches_Id == branch_id && ls.Tutor_UserAccounts_Id == tutor.u.Id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                                group ppi.Amount by ls.PayrollPaymentItems_Id into x
                                select new { PayrollPaymentItems_Id = x.Key, TotalAmount = x.ToList() }).ToList();
                if (payrolls.Count > 0)
                {
                    foreach (var payroll in payrolls)
                    {
                        var ppi = await db.PayrollPaymentItems.Where(x => x.Id == payroll.PayrollPaymentItems_Id.Value).FirstOrDefaultAsync();
                        tot_hours += ppi.Hour;
                        tot_payable += ppi.Amount;
                    }

                    list.Add(new TutorPayrollViewModels
                    {
                        TutorId = tutor.u.Id,
                        Name = tutor.u.Firstname + " " + tutor.u.Middlename + " " + tutor.u.Lastname,
                        TotalHours = string.Format("{0:N2}", tot_hours),
                        TotalPayable = string.Format("{0:N2}", tot_payable),
                        Details = "<a href='#' onclick='Details(\"" + branch_id.ToString() + "\",\"" + month + "\",\"" + year + "\",\"" + tutor.u.Id + "\")'>Details</a>"
                    });
                }

                //var sessions = await (from ls in db.LessonSessions
                //                      join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                //                      join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                //                      where si.Branches_Id == branch_id 
                //                        && ls.Tutor_UserAccounts_Id == tutor.u.Id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                //                      select new { ls }).ToListAsync();
                //if (sessions.Count > 0)
                //{
                //    decimal tot_payable = 0;
                //    foreach (var s in sessions)
                //    {
                //        tot_payable += s.ls.SessionHours * s.ls.HourlyRates_Rate;
                //    }

                //    list.Add(new TutorPayrollViewModels
                //    {
                //        TutorId = tutor.u.Id,
                //        Name = tutor.u.Firstname + " " + tutor.u.Middlename + " " + tutor.u.Lastname,
                //        TotalHours = string.Format("{0:N2}", sessions.Sum(x => x.ls.SessionHours)),
                //        TotalPayable = string.Format("{0:N2}", Math.Ceiling(tot_payable)),
                //        Details = "<a href='#' onclick='Details(\"" + branch_id.ToString() + "\",\"" + month + "\",\"" + year + "\",\"" + tutor.u.Id + "\")'>Details</a>"
                //    });
                //}
            }

            return Json(new { obj = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetDetails
        public async Task<JsonResult> GetDetails(Guid branch_id, int month, int year, string tutor_id)
        {
            var tutor = await db.User.FindAsync(tutor_id);
            decimal payroll_total = 0;
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));
            List<TutorPayrollDetailsViewModels> list = new List<TutorPayrollDetailsViewModels>();

            var payrolls = (from ppi in db.PayrollPaymentItems
                            join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
                            join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                            join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            where si.Branches_Id == branch_id && ls.Tutor_UserAccounts_Id == tutor_id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                            group ls by ls.PayrollPaymentItems_Id into x
                            select new { PayrollPaymentItems_Id = x.Key, LessonSession = x.ToList() }).ToList();
            foreach (var payroll in payrolls)
            {
                var ppi = await db.PayrollPaymentItems.Where(x => x.Id == payroll.PayrollPaymentItems_Id.Value).FirstOrDefaultAsync();
                if (!ppi.PayrollPayments_Id.HasValue)
                    payroll_total += ppi.Amount;

                list.Add(new TutorPayrollDetailsViewModels
                {
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(payroll.LessonSession.Select(x => x.Timestamp).FirstOrDefault(), TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Description = ppi.Description,
                    SessionHours = string.Format("{0:N2}", ppi.Hour),
                    HourlyRate = string.Format("{0:N2}", ppi.HourlyRate),
                    Amount = string.Format("{0:N0}", ppi.Amount),
                    Paid = ppi.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
                });
            }

            //var result = await (from ls in db.LessonSessions
            //                    join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
            //                    join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
            //                    join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
            //                    where si.Branches_Id == branch_id
            //                        && ls.Tutor_UserAccounts_Id == tutor_id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
            //                    orderby ls.Timestamp
            //                    select new { ls, u, sii }).ToListAsync();
            
            //foreach (var item in result)
            //{
            //    tutor_name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname;
            //    if (item.ls.PayrollPayments_Id == null)
            //    {
            //        payroll_total += item.ls.SessionHours * item.ls.HourlyRates_Rate;
            //    }
            //    list.Add(new TutorPayrollDetailsViewModels
            //    {
            //        LessonSessions_Id = item.ls.Id,
            //        Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.ls.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
            //        Description = item.sii.Description,
            //        SessionHours = string.Format("{0:N2}", item.ls.SessionHours),
            //        HourlyRate = string.Format("{0:N2}", item.ls.HourlyRates_Rate),
            //        Paid = item.ls.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
            //    });
            //}

            return Json(new
            {
                tutor_name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname,
                amount = string.Format("{0:N2}", payroll_total),
                obj = list
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetDetailsPayments - comment
        //public async Task<JsonResult> GetDetailsPayments(Guid payments_id)
        //{
        //    string tutor_name = ""; decimal payroll_total = 0;
        //    List<TutorPayrollDetailsViewModels> list = new List<TutorPayrollDetailsViewModels>();

        //    var payrolls = await (from pp in db.PayrollPayments
        //                          join ppi in db.PayrollPaymentItems on pp.Id equals ppi.PayrollPayments_Id
        //                          join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
        //                          join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
        //                          where pp.Id == payments_id
        //                          select new { pp, ppi, u, ls }).ToListAsync();
        //    foreach (var payroll in payrolls)
        //    {
        //        tutor_name = payroll.u.Firstname + " " + payroll.u.Middlename + " " + payroll.u.Lastname;
        //        if (!payroll.ppi.PayrollPayments_Id.HasValue)
        //            payroll_total += payroll.ppi.Amount;

        //        list.Add(new TutorPayrollDetailsViewModels
        //        {
        //            Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(payroll.ls.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
        //            Description = payroll.ppi.Description,
        //            SessionHours = string.Format("{0:N2}", payroll.ppi.Hour),
        //            HourlyRate = string.Format("{0:N2}", payroll.ppi.HourlyRate),
        //            Amount = string.Format("{0:N0}", payroll.ppi.Amount),
        //            Paid = payroll.ppi.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
        //        });
        //    }

        //    //var result = await (from pp in db.PayrollPayments
        //    //                    join ls in db.LessonSessions on pp.Id equals ls.PayrollPayments_Id
        //    //                    join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
        //    //                    join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
        //    //                    join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
        //    //                    where pp.Id == payments_id
        //    //                    orderby ls.Timestamp
        //    //                    select new { ls, u, sii }).ToListAsync();

        //    //string tutor_name = ""; decimal payroll_total = 0;
        //    //foreach (var item in result)
        //    //{
        //    //    tutor_name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname;
        //    //    payroll_total += item.ls.SessionHours * item.ls.HourlyRates_Rate;

        //    //    list.Add(new TutorPayrollDetailsViewModels
        //    //    {
        //    //        LessonSessions_Id = item.ls.Id,
        //    //        Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.ls.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
        //    //        Description = item.sii.Description,
        //    //        SessionHours = string.Format("{0:N2}", item.ls.SessionHours),
        //    //        HourlyRate = string.Format("{0:N2}", item.ls.HourlyRates_Rate),
        //    //        Paid = item.ls.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
        //    //    });
        //    //}

        //    return Json(new { tutor_name, amount = string.Format("{0:N2}", payroll_total), obj = list }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        #region SavePayrollPayments
        public async Task<JsonResult> SavePayrollPayments(Guid branch_id, int month, int year, string tutor_id, DateTime timestamp, decimal total_paid, string notes)
        {
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));

            var payrolls = (from ppi in db.PayrollPaymentItems
                            join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
                            join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                            join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                            join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                            where ppi.PayrollPayments_Id == null && si.Branches_Id == branch_id && ls.Tutor_UserAccounts_Id == tutor_id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                            group ls by ls.PayrollPaymentItems_Id into x
                            select new { PayrollPaymentItems_Id = x.Key, LessonSession = x.ToList() }).ToList();

            //var result = await (from ls in db.LessonSessions
            //                    join u in db.User on ls.Tutor_UserAccounts_Id equals u.Id
            //                    join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
            //                    join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
            //                    where si.Branches_Id == branch_id
            //                        && ls.Tutor_UserAccounts_Id == tutor_id && ls.Deleted == false && ls.PayrollPayments_Id == null
            //                        && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
            //                    orderby ls.Timestamp
            //                    select new { ls, u, sii }).ToListAsync();

            DateTime dateNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            string lastHex_string = db.PayrollPayments.AsNoTracking().Max(x => x.No);
            int lastHex_int = int.Parse(
                string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                System.Globalization.NumberStyles.HexNumber);
            PayrollPaymentsModels payrollPaymentsModels = new PayrollPaymentsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = TimeZoneInfo.ConvertTimeToUtc(new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, dateNow.Hour, dateNow.Minute, dateNow.Second)),
                No = (lastHex_int + 1).ToString("X5"),
                Amount = total_paid,
                Notes = notes,
                UserAccounts_Id = tutor_id,
                IsChecked = false,
                Cancelled = false
            };
            db.PayrollPayments.Add(payrollPaymentsModels);

            foreach (var payroll in payrolls)
            {
                var ppi = await db.PayrollPaymentItems.Where(x => x.Id == payroll.PayrollPaymentItems_Id.Value).FirstOrDefaultAsync();
                ppi.PayrollPayments_Id = payrollPaymentsModels.Id;
                db.Entry(ppi).State = EntityState.Modified;
            }

            //foreach (var item in result)
            //{
            //    LessonSessionsModels lessonSessionsModels = await db.LessonSessions.FindAsync(item.ls.Id);
            //    lessonSessionsModels.PayrollPayments_Id = payrollPaymentsModels.Id;
            //    db.Entry(lessonSessionsModels).State = EntityState.Modified;
            //}

            await db.SaveChangesAsync();
            return Json(new { status = "200", payroll_id = payrollPaymentsModels.Id }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Payment - comment
        //public async Task<JsonResult> Cancelled(Guid id)
        //{
        //    var payment = await db.PayrollPayments.FindAsync(id);
        //    payment.Cancelled = true;
        //    db.Entry(payment).State = EntityState.Modified;

        //    var sessions = await db.LessonSessions.Where(x => x.PayrollPayments_Id == payment.Id).ToListAsync();
        //    foreach (var item in sessions)
        //    {
        //        LessonSessionsModels lessonSessionsModels = await db.LessonSessions.FindAsync(item.Id);
        //        lessonSessionsModels.PayrollPayments_Id = null;
        //        db.Entry(lessonSessionsModels).State = EntityState.Modified;
        //    }

        //    await db.SaveChangesAsync();
        //    return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        #region Approve Payment
        public async Task<JsonResult> Approved(Guid id)
        {
            var payment = await db.PayrollPayments.FindAsync(id);
            payment.IsChecked = true;
            db.Entry(payment).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region CancelApproved
        public async Task<JsonResult> CancelApproved(Guid id)
        {
            var payment = await db.PayrollPayments.FindAsync(id);
            payment.IsChecked = false;
            db.Entry(payment).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult NewStudent()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View();
            }
        }

        public ActionResult Payroll()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View();
            }
        }

        public async Task<ActionResult> PayrollPayments()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var result = await (from pp in db.PayrollPayments
                                    join u in db.User on pp.UserAccounts_Id equals u.Id
                                    select new { pp, u }).ToListAsync();

                List<PayrollPaymentsViewModels> list = new List<PayrollPaymentsViewModels>();
                foreach (var item in result)
                {
                    list.Add(new PayrollPaymentsViewModels
                    {
                        Id = item.pp.Id,
                        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.pp.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                        No = item.pp.No,
                        Amount = item.pp.Amount,
                        Notes = item.pp.Notes,
                        Tutor = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                        Cancelled = item.pp.Cancelled,
                        IsChecked = item.pp.IsChecked,
                        Tutor_Id = item.u.Id
                    });
                }

                ViewBag.Cancel = p.IsGranted(User.Identity.Name, "payrollpayments_cancel");
                ViewBag.Approve = p.IsGranted(User.Identity.Name, "payrollpayments_approve");
                return View(list);
            }
        }

        public async Task<ActionResult> Print(Guid id, string tutor_id)
        {
            var payroll_payment = await (from pp in db.PayrollPayments
                                         join u in db.User on pp.UserAccounts_Id equals u.Id
                                         where pp.Id == id
                                         select new { pp, u }).FirstOrDefaultAsync();

            var result = await db.Database.SqlQuery<PayrollByStudent>(@"
                SELECT
                u.Id Student_Id, u.Firstname+' '+ISNULL(u.Middlename,'')+' '+ISNULL(u.Lastname,'') StudentName,y.TotalHours,z.TotalRate --CEILING(z.TotalRate) TotalRate
                FROM AspNetUsers u
                INNER JOIN (
	                SELECT si.Customer_UserAccounts_Id
	                FROM SaleInvoices si
	                INNER JOIN SaleInvoiceItems sii ON si.Id=sii.SaleInvoices_Id
	                INNER JOIN LessonSessions ls ON sii.Id=ls.SaleInvoiceItems_Id
	                INNER JOIN PayrollPaymentItems ppi ON ls.PayrollPaymentItems_Id=ppi.Id
		            INNER JOIN PayrollPayments pp ON ppi.PayrollPayments_Id=pp.Id
	                WHERE ls.Deleted=0 AND ls.Tutor_UserAccounts_Id='" + tutor_id + "' AND pp.Id='" + id + @"'
	                GROUP BY si.Customer_UserAccounts_Id
                ) v ON u.Id=v.Customer_UserAccounts_Id
                INNER JOIN (
	                SELECT si.Customer_UserAccounts_Id
	                FROM SaleInvoices si
	                INNER JOIN SaleInvoiceItems sii ON si.Id=sii.SaleInvoices_Id
	                INNER JOIN LessonSessions ls ON sii.Id=ls.SaleInvoiceItems_Id
                    INNER JOIN PayrollPaymentItems ppi ON ls.PayrollPaymentItems_Id=ppi.Id
		            INNER JOIN PayrollPayments pp ON ppi.PayrollPayments_Id=pp.Id
	                WHERE ls.Deleted=0 AND ls.Tutor_UserAccounts_Id='" + tutor_id + "' AND pp.Id='" + id + @"'
	                GROUP BY si.Customer_UserAccounts_Id
                ) x ON u.Id=x.Customer_UserAccounts_Id
                INNER JOIN (
	                SELECT si.Customer_UserAccounts_Id,SUM(ls.SessionHours) TotalHours
	                FROM SaleInvoices si
	                INNER JOIN SaleInvoiceItems sii ON si.Id=sii.SaleInvoices_Id
	                INNER JOIN LessonSessions ls ON sii.Id=ls.SaleInvoiceItems_Id
                    INNER JOIN PayrollPaymentItems ppi ON ls.PayrollPaymentItems_Id=ppi.Id
		            INNER JOIN PayrollPayments pp ON ppi.PayrollPayments_Id=pp.Id
	                WHERE ls.Deleted=0 AND ls.Tutor_UserAccounts_Id='" + tutor_id + "' AND pp.Id='" + id + @"'
	                GROUP BY si.Customer_UserAccounts_Id
                ) y ON x.Customer_UserAccounts_Id=y.Customer_UserAccounts_Id
                INNER JOIN (
	                SELECT usr.Id,SUM(a.Fee) TotalRate
	                FROM AspNetUsers usr
	                INNER JOIN (
		                SELECT si.Customer_UserAccounts_Id,ls.SessionHours*ls.HourlyRates_Rate Fee
		                FROM SaleInvoices si
		                INNER JOIN SaleInvoiceItems sii ON si.Id=sii.SaleInvoices_Id
		                INNER JOIN LessonSessions ls ON sii.Id=ls.SaleInvoiceItems_Id
                        INNER JOIN PayrollPaymentItems ppi ON ls.PayrollPaymentItems_Id=ppi.Id
		                INNER JOIN PayrollPayments pp ON ppi.PayrollPayments_Id=pp.Id
		                WHERE ls.Deleted=0 AND ls.Tutor_UserAccounts_Id='" + tutor_id + "' AND pp.Id='" + id + @"'
	                ) a ON usr.Id=a.Customer_UserAccounts_Id
	                GROUP BY usr.Id
                ) z ON y.Customer_UserAccounts_Id=z.Id
                ORDER BY u.Firstname
            ").ToListAsync();

            ReceiptPayrollViewModels model = new ReceiptPayrollViewModels
            {
                PayrollDate = TimeZoneInfo.ConvertTimeFromUtc(payroll_payment.pp.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                TutorName = payroll_payment.u.Firstname + " " + payroll_payment.u.Middlename + " " + payroll_payment.u.Lastname,
                ListPayroll = result
            };

            return View(model);
        }
    }
}