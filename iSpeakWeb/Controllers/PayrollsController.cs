using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iSpeak.Models;
using iSpeak.Common;
using LIBUtil;

namespace iSpeak.Controllers
{
    public class PayrollsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        /* INDEX **********************************************************************************************************************************************/

        // GET: Payrolls
        public ActionResult Index(int? year, int? month, string search, string periodChange)
        {
            DateTime StartDate = Helper.setFilterViewBag(this, year, month, search, periodChange);
            List<PayrollsModel> models = get(StartDate, (DateTime)Util.getAsEndDate(StartDate.AddMonths(1).AddDays(-1)));

            return View(models);
        }

        /* METHODS ********************************************************************************************************************************************/

        private List<PayrollsModel> get(DateTime StartDate, DateTime EndDate)
        {
            List<PayrollsModel> models = db.Database.SqlQuery<PayrollsModel>(@"
                        SELECT Payrolls.*,
							ISNULL(Due.Amount,0) AS DueAmount,
							AspNetUsers.Firstname + ' ' + ISNULL(AspNetUsers.Middlename,'') + ' ' + ISNULL(AspNetUsers.LastName,'') AS Tutor_UserAccounts_FullName
						FROM (
								SELECT UserAccounts_Id AS Tutor_UserAccounts_Id,
									SUM(Hour) AS TotalHours,
									SUM(Amount) AS PayableAmount
								FROM PayrollPaymentItems
								WHERE PayrollPaymentItems.Timestamp >= @StartDate AND PayrollPaymentItems.Timestamp <= @EndDate
                                    AND PayrollPaymentItems.Branches_Id = @Branches_Id
								GROUP BY UserAccounts_Id						
							) Payrolls
							LEFT JOIN AspNetUsers ON AspNetUsers.Id = Payrolls.Tutor_UserAccounts_Id
							LEFT JOIN (
									SELECT UserAccounts_Id AS Tutor_UserAccounts_Id,
										SUM(Amount) AS Amount
									FROM PayrollPaymentItems
									WHERE PayrollPaymentItems.Timestamp >= @StartDate AND PayrollPaymentItems.Timestamp <= @EndDate
                                        AND PayrollPaymentItems.Branches_Id = @Branches_Id
										AND PayrollPayments_Id IS NULL
									GROUP BY UserAccounts_Id						
								) Due ON Due.Tutor_UserAccounts_Id = Payrolls.Tutor_UserAccounts_Id
						ORDER BY AspNetUsers.Firstname + ' ' + ISNULL(AspNetUsers.Middlename,'') + ' ' + ISNULL(AspNetUsers.LastName,'') ASC
                    ",
                    DBConnection.getSqlParameter("Branches_Id", User.Identity.GetBranches_Id()),
                    DBConnection.getSqlParameter("StartDate", StartDate),
                    DBConnection.getSqlParameter("EndDate", EndDate)
                ).ToList();

            return models;
        }

    }
}