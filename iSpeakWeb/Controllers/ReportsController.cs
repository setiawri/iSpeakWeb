using iSpeak.Common;
using iSpeak.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        #region GetNewStudent
        public async Task<JsonResult> GetNewStudent(string branch_id, int month, int year)
        {
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));
            List<NewStudentBeforeViewModels> list = await db.Database.SqlQuery<NewStudentBeforeViewModels>(@"
                SELECT
                u.Id StudentId,u.Firstname+' '+ISNULL(u.Middlename,'')+' '+ISNULL(u.Lastname,'') Name,inv.qty_lesson Qty
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

            var oldStudent = await (from si in db.SaleInvoices
                                    where si.Timestamp < dateFrom
                                    group si by si.Customer_UserAccounts_Id into g
                                    select new { StudentId = g.Key, Invoices = g.ToList() }).ToListAsync();
            List<NewStudentViewModels> list_filtered = new List<NewStudentViewModels>();
            foreach (var s in list)
            {
                var check = oldStudent.Where(x => x.StudentId == s.StudentId).ToList();
                if (check.Count == 0)
                {
                    list_filtered.Add(new NewStudentViewModels
                    {
                        Name = s.Name,
                        Qty = s.Qty
                    });
                }
            }

            return Json(new { obj = list_filtered }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region ChangePeriod
        public JsonResult ChangePeriod(string action, int month, int year)
        {
            DateTime tanggal = new DateTime(year, month, 1);
            if (action == "back")
            {
                tanggal = tanggal.AddMonths(-1);
            }
            else
            {
                tanggal = tanggal.AddMonths(1);
            }

            return Json(new { newMonth = tanggal.Month, newYear = tanggal.Year }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetTutorPayroll
        public async Task<JsonResult> GetTutorPayroll(Guid branch_id, int month, int year)
        {
            DateTime dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, 1, 0, 0, 0));
            DateTime dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59));
            List<TutorPayrollViewModels> list = new List<TutorPayrollViewModels>();

            //var tutors = await (from u in db.User
            //                    join ur in db.UserRole on u.Id equals ur.UserId
            //                    join r in db.Role on ur.RoleId equals r.Id
            //                    where r.Name.ToLower() == "tutor"
            //                    orderby u.Firstname
            //                    select new { u }).ToListAsync();
            var tutors = await db.User.ToListAsync();

            foreach (var tutor in tutors)
            {
                decimal tot_hours = 0;
                decimal tot_payable = 0;
                var payrolls = (from ppi in db.PayrollPaymentItems
                                join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
                                join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                                join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                                where si.Branches_Id == branch_id && ls.Tutor_UserAccounts_Id == tutor.Id && ls.Deleted == false && ls.Timestamp >= dateFrom && ls.Timestamp <= dateTo
                                group ppi.Amount by ls.PayrollPaymentItems_Id into x
                                select new { PayrollPaymentItems_Id = x.Key, TotalAmount = x.ToList() }).ToList();

                var payroll_manual = await db.PayrollPaymentItems
                    .Where(x => x.UserAccounts_Id == tutor.Id && x.Hour == 0 && x.Description != "" && x.Timestamp >= dateFrom && x.Timestamp <= dateTo)
                    .OrderBy(x => x.Timestamp).ToListAsync();

                if (payrolls.Count > 0)
                {
                    foreach (var payroll in payrolls)
                    {
                        var ppi = await db.PayrollPaymentItems.Where(x => x.Id == payroll.PayrollPaymentItems_Id.Value).FirstOrDefaultAsync();
                        tot_hours += ppi.Hour;
                        tot_payable += ppi.Amount;
                    }

                    if (payroll_manual.Count > 0)
                    {
                        tot_payable += payroll_manual.Sum(x => x.Amount);
                    }

                    list.Add(new TutorPayrollViewModels
                    {
                        TutorId = tutor.Id,
                        Name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname,
                        TotalHours = string.Format("{0:N2}", tot_hours),
                        TotalPayable = string.Format("{0:N2}", tot_payable),
                        Details = "<a href='javascript:void(0)' onclick='Details(\"" + branch_id.ToString() + "\",\"" + month + "\",\"" + year + "\",\"" + tutor.Id + "\")'>Details</a>"
                    });
                }
                else
                {
                    if (payroll_manual.Count > 0)
                    {
                        list.Add(new TutorPayrollViewModels
                        {
                            TutorId = tutor.Id,
                            Name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname,
                            TotalHours = string.Format("{0:N2}", tot_hours),
                            TotalPayable = string.Format("{0:N2}", payroll_manual.Sum(x => x.Amount)),
                            Details = "<a href='#' onclick='Details(\"" + branch_id.ToString() + "\",\"" + month + "\",\"" + year + "\",\"" + tutor.Id + "\")'>Details</a>"
                        });
                    }
                }
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

                string students = "";
                foreach (var s in payroll.LessonSession)
                {
                    Guid sale_inv_id = db.SaleInvoiceItems.Where(x => x.Id == s.SaleInvoiceItems_Id).FirstOrDefault().SaleInvoices_Id;
                    string student_id = db.SaleInvoices.Where(x => x.Id == sale_inv_id).FirstOrDefault().Customer_UserAccounts_Id;
                    string student_first_name = db.User.Where(x => x.Id == student_id).FirstOrDefault().Firstname;
                    students += string.IsNullOrEmpty(students) ? student_first_name : ", " + student_first_name;
                }

                list.Add(new TutorPayrollDetailsViewModels
                {
                    Id = payroll.PayrollPaymentItems_Id.Value,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", payroll.LessonSession.Select(x => x.Timestamp).FirstOrDefault()), //string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(payroll.LessonSession.Select(x => x.Timestamp).FirstOrDefault(), TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Description = students,
                    SessionHours = string.Format("{0:N2}", ppi.Hour),
                    HourlyRate = ppi.PayrollPayments_Id == null ? "<a href='javascript:void(0)' data-toggle='modal' data-target='#modal_edit' onclick='EditPayrate(\"" + payroll.PayrollPaymentItems_Id.Value + "\",\"" + string.Format("{0:N2}", ppi.Hour) + "\",\"" + string.Format("{0:N2}", ppi.HourlyRate) + "\",\"" + string.Format("{0:N0}", ppi.TutorTravelCost) + "\")'>" + string.Format("{0:N2}", ppi.HourlyRate) + "</a>" : string.Format("{0:N2}", ppi.HourlyRate),
                    TravelCost = string.Format("{0:N0}", ppi.TutorTravelCost),
                    Amount = string.Format("{0:N0}", ppi.Amount),
                    Paid = ppi.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
                });
            }

            var payroll_manual = await db.PayrollPaymentItems
                .Where(x => x.UserAccounts_Id == tutor_id && x.Hour == 0 && x.Description != "" && x.Timestamp >= dateFrom && x.Timestamp <= dateTo)
                .OrderBy(x => x.Timestamp).ToListAsync();
            foreach (var payroll in payroll_manual)
            {
                if (!payroll.PayrollPayments_Id.HasValue)
                    payroll_total += payroll.Amount;

                list.Add(new TutorPayrollDetailsViewModels
                {
                    Id = payroll.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", payroll.Timestamp.Value), //string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(payroll.Timestamp.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Description = payroll.Description,
                    SessionHours = string.Empty,
                    HourlyRate = string.Empty,
                    TravelCost = string.Empty,
                    Amount = string.Format("{0:N0}", payroll.Amount),
                    Paid = payroll.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
                });
            }

            return Json(new
            {
                tutor_name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname,
                amount = string.Format("{0:N2}", payroll_total),
                obj = list
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region SaveManualPayroll
        public async Task<JsonResult> SaveManualPayroll(string tutor_id, DateTime timestamp, decimal amount, string description, decimal payroll_total)
        {
            PayrollPaymentItemsModels payrollPaymentItemsModels = new PayrollPaymentItemsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second),
                Description = description,
                Amount = amount,
                UserAccounts_Id = tutor_id
            };
            db.PayrollPaymentItems.Add(payrollPaymentItemsModels);

            await db.SaveChangesAsync();
            return Json(new
            {
                status = "200",
                timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", payrollPaymentItemsModels.Timestamp),
                description,
                amount = string.Format("{0:N0}", payrollPaymentItemsModels.Amount),
                total = string.Format("{0:N0}", payroll_total + payrollPaymentItemsModels.Amount),
                paid = "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region EditPayrate
        public async Task<JsonResult> EditPayrate(Guid id, decimal hour, decimal rate, int travel, decimal payroll_total)
        {
            var ppi = await db.PayrollPaymentItems.FindAsync(id);
            decimal amount_before = ppi.Amount;
            ppi.Hour = hour;
            ppi.HourlyRate = rate;
            ppi.TutorTravelCost = travel;
            ppi.Amount = (hour * rate) + travel;
            db.Entry(ppi).State = EntityState.Modified;
            
            await db.SaveChangesAsync();
            return Json(new
            {
                status = "200",
                timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", ppi.Timestamp),
                description = ppi.Description,
                session_hour = string.Format("{0:N2}", ppi.Hour),
                hourly_rate = string.Format("{0:N2}", ppi.HourlyRate),
                tutor_travel = string.Format("{0:N0}", ppi.TutorTravelCost),
                amount = string.Format("{0:N0}", ppi.Amount),
                total = string.Format("{0:N0}", payroll_total - amount_before + ppi.Amount),
                paid = "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>"
            }, JsonRequestBehavior.AllowGet);
        }
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
            
            DateTime dateNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            string lastHex_string = db.PayrollPayments.AsNoTracking().Max(x => x.No);
            int lastHex_int = int.Parse(
                string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                System.Globalization.NumberStyles.HexNumber);
            PayrollPaymentsModels payrollPaymentsModels = new PayrollPaymentsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second),
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

            var payrolls_manual = await db.PayrollPaymentItems.Where(x => x.PayrollPayments_Id == null && x.Hour == 0 && x.UserAccounts_Id == tutor_id && x.Timestamp >= dateFrom && x.Timestamp <= dateTo).ToListAsync();
            foreach (var payroll in payrolls_manual)
            {
                var ppi = await db.PayrollPaymentItems.FindAsync(payroll.Id);
                ppi.PayrollPayments_Id = payrollPaymentsModels.Id;
                db.Entry(ppi).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();
            return Json(new { status = "200", payroll_id = payrollPaymentsModels.Id }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region GetDetailsPayments
        public async Task<JsonResult> GetDetailsPayments(Guid payments_id)
        {
            string tutor_name = ""; decimal payroll_total = 0;
            List<TutorPayrollDetailsViewModels> list = new List<TutorPayrollDetailsViewModels>();
            
            var payrolls = await (from pp in db.PayrollPayments
                                  join ppi in db.PayrollPaymentItems on pp.Id equals ppi.PayrollPayments_Id
                                  join ls in db.LessonSessions on ppi.Id equals ls.PayrollPaymentItems_Id
                                  join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                                  join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                                  join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                                  where pp.Id == payments_id
                                  group ls by ls.PayrollPaymentItems_Id into x
                                  select new { PayrollPaymentItems_Id = x.Key, LessonSession = x.ToList() }).ToListAsync();
            foreach (var payroll in payrolls)
            {
                var tutor = await db.User.FindAsync(payroll.LessonSession.Select(x => x.Tutor_UserAccounts_Id).FirstOrDefault());
                tutor_name = tutor.Firstname + " " + tutor.Middlename + " " + tutor.Lastname;

                var ppi = await db.PayrollPaymentItems.Where(x => x.Id == payroll.PayrollPaymentItems_Id.Value).FirstOrDefaultAsync();
                //if (!ppi.PayrollPayments_Id.HasValue)
                    payroll_total += ppi.Amount;
                
                string students = "";
                foreach (var s in payroll.LessonSession)
                {
                    Guid sale_inv_id = db.SaleInvoiceItems.Where(x => x.Id == s.SaleInvoiceItems_Id).FirstOrDefault().SaleInvoices_Id;
                    string student_id = db.SaleInvoices.Where(x => x.Id == sale_inv_id).FirstOrDefault().Customer_UserAccounts_Id;
                    string student_first_name = db.User.Where(x => x.Id == student_id).FirstOrDefault().Firstname;
                    students += string.IsNullOrEmpty(students) ? student_first_name : ", " + student_first_name;
                }

                list.Add(new TutorPayrollDetailsViewModels
                {
                    Id = payroll.PayrollPaymentItems_Id.Value,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", payroll.LessonSession.Select(x => x.Timestamp).FirstOrDefault()), //string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(payroll.LessonSession.Select(x => x.Timestamp).FirstOrDefault(), TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Description = students,
                    SessionHours = string.Format("{0:N2}", ppi.Hour),
                    HourlyRate = string.Format("{0:N2}", ppi.HourlyRate),
                    Amount = string.Format("{0:N0}", ppi.Amount),
                    Paid = ppi.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
                });
            }

            var payroll_manual = await (from pp in db.PayrollPayments
                                        join ppi in db.PayrollPaymentItems on pp.Id equals ppi.PayrollPayments_Id
                                        where pp.Id == payments_id && ppi.Hour == 0
                                        select new { pp, ppi }).ToListAsync();
            foreach (var payroll in payroll_manual)
            {
                //if (!payroll.PayrollPayments_Id.HasValue)
                    payroll_total += payroll.ppi.Amount;

                list.Add(new TutorPayrollDetailsViewModels
                {
                    Id = payroll.ppi.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", payroll.ppi.Timestamp.Value),
                    Description = payroll.ppi.Description,
                    SessionHours = string.Empty,
                    HourlyRate = string.Empty,
                    Amount = string.Format("{0:N0}", payroll.ppi.Amount),
                    Paid = payroll.ppi.PayrollPayments_Id == null ? "<span class='text-danger'><i class='icon-cancel-circle2'></i></span>" : "<span class='text-primary'><i class='icon-checkmark'></i></span>"
                });
            }

            return Json(new { tutor_name, amount = string.Format("{0:N2}", payroll_total), obj = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Payment
        public async Task<JsonResult> Cancelled(Guid id, string notes)
        {
            var payment = await db.PayrollPayments.FindAsync(id);
            payment.Cancelled = true;
            payment.Notes_Cancel = notes;
            db.Entry(payment).State = EntityState.Modified;

            var items = await db.PayrollPaymentItems.Where(x => x.PayrollPayments_Id == id).ToListAsync();
            foreach (var item in items)
            {
                var payment_item = await db.PayrollPaymentItems.FindAsync(item.Id);
                payment_item.PayrollPayments_Id = null;
                db.Entry(payment_item).State = EntityState.Modified;
            }

            //var sessions = await db.LessonSessions.Where(x => x.PayrollPayments_Id == payment.Id).ToListAsync();
            //foreach (var item in sessions)
            //{
            //    LessonSessionsModels lessonSessionsModels = await db.LessonSessions.FindAsync(item.Id);
            //    lessonSessionsModels.PayrollPayments_Id = null;
            //    db.Entry(lessonSessionsModels).State = EntityState.Modified;
            //}

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
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
        #region GetProfitLoss
        public async Task<JsonResult> GetProfitLoss(Guid branch_id, DateTime start, DateTime end)
        {
            DateTime fromDate = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            DateTime toDate = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            DateTime fromDateUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(start.Year, start.Month, start.Day, 0, 0, 0));
            DateTime toDateUtc = TimeZoneInfo.ConvertTimeToUtc(new DateTime(end.Year, end.Month, end.Day, 23, 59, 59));

            List<ProfitLossViewModels> list = new List<ProfitLossViewModels>();
            decimal total_payment = 0;
            decimal total_petty = 0;
            decimal total_expense = 0;

            var payments = await db.Payments.Where(x => x.Cancelled == false && x.Timestamp >= fromDateUtc && x.Timestamp <= toDateUtc).ToListAsync();
            foreach (var p in payments)
            {
                var check_branch = await (from pay in db.Payments
                                          join pi in db.PaymentItems on pay.Id equals pi.Payments_Id
                                          join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                                          where pay.Id == p.Id && si.Branches_Id == branch_id
                                          select new { si }).ToListAsync();
                if (check_branch.Count > 0)
                {
                    list.Add(new ProfitLossViewModels
                    {
                        Id = p.Id,
                        Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(p.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                        Status = "<span class='badge badge-success d-block'>Payment</span>",
                        Description = "#" + p.No + " Payment Invoice",
                        Amount = string.Format("{0:N2}", p.CashAmount + p.DebitAmount + p.ConsignmentAmount)
                    });
                    total_payment += p.CashAmount + p.DebitAmount + p.ConsignmentAmount;
                }
            }

            var pettycashs = await db.PettyCashRecords.Where(x => x.ExpenseCategories_Id != null && x.Branches_Id == branch_id && x.Timestamp >= fromDateUtc && x.Timestamp <= toDateUtc).ToListAsync();
            foreach (var p in pettycashs)
            {
                list.Add(new ProfitLossViewModels
                {
                    Id = p.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(p.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))),
                    Status = "<span class='badge badge-warning d-block'>Petty Cash</span>",
                    Description = "#" + p.No + " " + p.Notes,
                    Amount = string.Format("{0:N2}", p.Amount)
                });
                total_petty += p.Amount;
            }

            var expenses = await db.Expenses.Where(x => x.Branches_Id == branch_id && x.Timestamp >= fromDate && x.Timestamp <= toDate).ToListAsync();
            foreach (var e in expenses)
            {
                list.Add(new ProfitLossViewModels
                {
                    Id = e.Id,
                    Timestamp = string.Format("{0:yyyy/MM/dd HH:mm}", e.Timestamp),
                    Status = "<span class='badge badge-danger d-block'>Expense</span>",
                    Description = e.Description,
                    Amount = string.Format("{0:N2}", e.Amount)
                });
                total_expense += e.Amount;
            }

            return Json(new
            {
                list,
                payment = string.Format("{0:N2}", total_payment),
                petty = string.Format("{0:N2}", total_petty),
                expense = string.Format("{0:N2}", total_expense),
                total = string.Format("{0:N2}", total_payment + total_petty + total_expense)
            }, JsonRequestBehavior.AllowGet);
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
                        Timestamp = item.pp.Timestamp,
                        No = item.pp.No,
                        Amount = item.pp.Amount,
                        Notes = item.pp.Notes,
                        Tutor = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname,
                        Cancelled = item.pp.Cancelled,
                        IsChecked = item.pp.IsChecked,
                        Tutor_Id = item.u.Id,
                        Notes_Cancel = item.pp.Notes_Cancel
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

            var result_manual = await (from pp in db.PayrollPayments
                                       join ppi in db.PayrollPaymentItems on pp.Id equals ppi.PayrollPayments_Id
                                       where pp.Id == id && ppi.Hour == 0
                                       select new PayrollManualReceipt
                                       {
                                           Description = ppi.Description,
                                           Amount = ppi.Amount
                                       }).ToListAsync();

            ReceiptPayrollViewModels model = new ReceiptPayrollViewModels
            {
                PayrollDate = payroll_payment.pp.Timestamp,
                TutorName = payroll_payment.u.Firstname + " " + payroll_payment.u.Middlename + " " + payroll_payment.u.Lastname,
                ListPayroll = result,
                ListPayrollManual = result_manual
            };

            return View(model);
        }

        private decimal CalculateProfitLoss(Guid branch_id, DateTime fromDateUtc, DateTime toDateUtc, DateTime fromDate, DateTime toDate)
        {
            decimal total_profit_loss = 0;
            var payments = db.Payments.Where(x => x.Cancelled == false && x.Timestamp >= fromDateUtc && x.Timestamp <= toDateUtc).ToList();
            foreach (var p in payments)
            {
                var check_branch = (from pay in db.Payments
                                         join pi in db.PaymentItems on pay.Id equals pi.Payments_Id
                                         join si in db.SaleInvoices on pi.ReferenceId equals si.Id
                                         where pay.Id == p.Id && si.Branches_Id == branch_id
                                         select new { si }).ToList();
                if (check_branch.Count > 0)
                {
                    total_profit_loss += p.CashAmount + p.DebitAmount + p.ConsignmentAmount;
                }
            }

            var pettycashs = db.PettyCashRecords.Where(x => x.ExpenseCategories_Id != null && x.Branches_Id == branch_id && x.Timestamp >= fromDateUtc && x.Timestamp <= toDateUtc).ToList();
            foreach (var p in pettycashs)
            {
                total_profit_loss += p.Amount;
            }

            var expenses = db.Expenses.Where(x => x.Branches_Id == branch_id && x.Timestamp >= fromDate && x.Timestamp <= toDate).ToList();
            foreach (var e in expenses)
            {
                total_profit_loss += e.Amount;
            }

            return total_profit_loss;
        }

        public async Task<ActionResult> ProfitLoss()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                #region Chart JS
                ViewBag.ChartLabel = "'"
                        + DateTime.UtcNow.AddMonths(-5).ToString("MMM-yy") + "','"
                        + DateTime.UtcNow.AddMonths(-4).ToString("MMM-yy") + "','"
                        + DateTime.UtcNow.AddMonths(-3).ToString("MMM-yy") + "','"
                        + DateTime.UtcNow.AddMonths(-2).ToString("MMM-yy") + "','"
                        + DateTime.UtcNow.AddMonths(-1).ToString("MMM-yy") + "','"
                        + DateTime.UtcNow.ToString("MMM-yy") + "'";

                var start5MonthAgo = new DateTime(DateTime.UtcNow.AddMonths(-5).Year, DateTime.UtcNow.AddMonths(-5).Month, 1);
                var start4MonthAgo = new DateTime(DateTime.UtcNow.AddMonths(-4).Year, DateTime.UtcNow.AddMonths(-4).Month, 1);
                var start3MonthAgo = new DateTime(DateTime.UtcNow.AddMonths(-3).Year, DateTime.UtcNow.AddMonths(-3).Month, 1);
                var start2MonthAgo = new DateTime(DateTime.UtcNow.AddMonths(-2).Year, DateTime.UtcNow.AddMonths(-2).Month, 1);
                var start1MonthAgo = new DateTime(DateTime.UtcNow.AddMonths(-1).Year, DateTime.UtcNow.AddMonths(-1).Month, 1);
                var startThisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                var last5MonthAgo = start4MonthAgo.AddDays(-1);
                var last4MonthAgo = start3MonthAgo.AddDays(-1);
                var last3MonthAgo = start2MonthAgo.AddDays(-1);
                var last2MonthAgo = start1MonthAgo.AddDays(-1);
                var last1MonthAgo = startThisMonth.AddDays(-1);
                var lastThisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));

                var user_login = await db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefaultAsync();
                var branch_id = user_login.Branches_Id;
                ViewBag.ChartData =
                    CalculateProfitLoss(branch_id, start5MonthAgo, last5MonthAgo, start5MonthAgo.AddHours(7), last5MonthAgo.AddHours(7)) + ","
                    + CalculateProfitLoss(branch_id, start4MonthAgo, last4MonthAgo, start4MonthAgo.AddHours(7), last4MonthAgo.AddHours(7)) + ","
                    + CalculateProfitLoss(branch_id, start3MonthAgo, last3MonthAgo, start3MonthAgo.AddHours(7), last3MonthAgo.AddHours(7)) + ","
                    + CalculateProfitLoss(branch_id, start2MonthAgo, last2MonthAgo, start2MonthAgo.AddHours(7), last2MonthAgo.AddHours(7)) + ","
                    + CalculateProfitLoss(branch_id, start1MonthAgo, last1MonthAgo, start1MonthAgo.AddHours(7), last1MonthAgo.AddHours(7)) + ","
                    + CalculateProfitLoss(branch_id, startThisMonth, lastThisMonth, startThisMonth.AddHours(7), lastThisMonth.AddHours(7));
                #endregion

                ViewBag.initDateStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0);
                ViewBag.initDateEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month), 23, 59, 59);
                return View();
            }
        }

        public ActionResult SendEmails()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendEmails([Bind(Include = "InterestLanguage,From,To,Subject,Footer,Password")] EmailSenderViewModels emailSenderViewModels, HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("Image", "Image file is required.");
            }

            //check to mail addressed
            List<string> list_mailTo = new List<string>();
            var customers = await (from u in db.User
                                   join ur in db.UserRole on u.Id equals ur.UserId
                                   join r in db.Role on ur.RoleId equals r.Id
                                   where r.Name.ToLower() == "student"
                                   select new { u }).ToListAsync();
            foreach (var customer in customers)
            {
                if (!string.IsNullOrEmpty(customer.u.Email))
                {
                    if (!string.IsNullOrEmpty(emailSenderViewModels.InterestLanguage))
                    {
                        if (!string.IsNullOrEmpty(customer.u.Interest))
                        {
                            var interest = JsonConvert.DeserializeObject<List<InterestViewModels>>(customer.u.Interest);
                            if (interest.Count > 0)
                            {
                                foreach (var i in interest)
                                {
                                    if (i.Languages_Id == emailSenderViewModels.InterestLanguage)
                                    {
                                        list_mailTo.Add(customer.u.Email);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        list_mailTo.Add(customer.u.Email);
                    }
                }
            }

            if (list_mailTo.Count == 0)
            {
                ModelState.AddModelError("None", "None of Email address selected.");
            }

            if (ModelState.IsValid)
            {
                string Dir = Server.MapPath("~/assets/email/");
                if (!Directory.Exists(Dir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(Dir);
                }
                string namaFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fullPath = Path.Combine(Dir, namaFile);
                file.SaveAs(fullPath);

                using (var client = new SmtpClient())
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(emailSenderViewModels.From);
                    mail.To.Add(string.Join(",", list_mailTo));
                    mail.Subject = emailSenderViewModels.Subject;
                    mail.IsBodyHtml = true;

                    //mail.Body = string.Format("<h2>{0}</h2><img src='{1}' alt='iSpeak' title='iSpeak' style='display:block' />"
                    //    , emailSenderViewModels.Body
                    //    , HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + ":" + HttpContext.Request.Url.Port + "/assets/email/" + namaFile);

                    var inlineImage = new LinkedResource(fullPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };

                    string htmlBody = string.Format(@"
                        <img src=""cid:{0}"" /><br />
                        <b>{1}</b>
                    ", inlineImage.ContentId, emailSenderViewModels.Footer.Replace("\n", "<br />"));

                    var view = AlternateView.CreateAlternateViewFromString(htmlBody, null, System.Net.Mime.MediaTypeNames.Text.Html);
                    view.LinkedResources.Add(inlineImage);
                    mail.AlternateViews.Add(view);

                    Attachment att = new Attachment(fullPath);
                    att.ContentDisposition.Inline = true;
                    att.ContentDisposition.FileName = namaFile;
                    mail.Attachments.Add(att);

                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.Credentials = new System.Net.NetworkCredential(emailSenderViewModels.From, emailSenderViewModels.Password);
                    client.EnableSsl = true;
                    client.Send(mail);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.listLanguages = new SelectList(db.Languages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(emailSenderViewModels);
        }
    }
}