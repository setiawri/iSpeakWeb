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
    public class PayrollPaymentItemsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        /* INDEX **********************************************************************************************************************************************/

        // GET: PayrollPaymentItems
        public ActionResult Index(int year, int month, string search, string TutorId)
        {
            DateTime StartDate = Helper.setFilterViewBag(this, year, month, search, null);
            List<PayrollPaymentItemsModel> models = get(new Guid(TutorId), StartDate, (DateTime)Util.getAsEndDate(StartDate.AddMonths(1).AddDays(-1)));

            ViewBag.PayableAmount = string.Format("{0:N0}", models.Sum(x => x.Amount));
            ViewBag.DueAmount = string.Format("{0:N0}", models.Where(x => x.PayrollPayments_Id == null).Sum(x => x.Amount));
            if(models.Count > 0)
            {
                ViewBag.Title = models[0].Tutor_UserAccounts_FullName;
                ViewBag.Tutor_UserAccounts_Id = models[0].UserAccounts_Id;
            }

            return View(models);
        }

        /* METHODS ********************************************************************************************************************************************/

        private List<PayrollPaymentItemsModel> get(Guid? TutorId, DateTime StartDate, DateTime EndDate)
        {
            List<PayrollPaymentItemsModel> models = db.Database.SqlQuery<PayrollPaymentItemsModel>(@"
                        SELECT PayrollPaymentItems.*,
							TutorUserAccount.Firstname + ' ' + ISNULL(TutorUserAccount.Middlename,'') + ' ' + ISNULL(TutorUserAccount.LastName,'') AS Tutor_UserAccounts_FullName,
							StudentUserAccount.Firstname AS Student_UserAccounts_FirstName
						FROM PayrollPaymentItems
							LEFT JOIN LessonSessions ON LessonSessions.PayrollPaymentItems_Id = PayrollPaymentItems.Id
							LEFT JOIN SaleInvoiceItems ON SaleInvoiceItems.Id = LessonSessions.SaleInvoiceItems_Id
							LEFT JOIN SaleInvoices ON SaleInvoices.Id = SaleInvoiceItems.SaleInvoices_Id
							LEFT JOIN AspNetUsers TutorUserAccount ON TutorUserAccount.Id = PayrollPaymentItems.UserAccounts_Id
							LEFT JOIN AspNetUsers StudentUserAccount ON StudentUserAccount.Id = SaleInvoices.Customer_UserAccounts_Id
						WHERE PayrollPaymentItems.Timestamp >= @StartDate AND PayrollPaymentItems.Timestamp <= @EndDate
                            AND PayrollPaymentItems.Branches_Id = @Branches_Id
							AND (@Tutor_UserAccounts_Id IS NULL OR PayrollPaymentItems.UserAccounts_Id = @Tutor_UserAccounts_Id)
						ORDER BY StudentUserAccount.Firstname + ' ' + ISNULL(StudentUserAccount.Middlename,'') + ' ' + ISNULL(StudentUserAccount.LastName,'') ASC, PayrollPaymentItems.TimeStamp ASC
                    ",
                    DBConnection.getSqlParameter("Tutor_UserAccounts_Id", TutorId),
                    DBConnection.getSqlParameter("Branches_Id", User.Identity.GetBranches_Id()),
                    DBConnection.getSqlParameter("StartDate", StartDate),
                    DBConnection.getSqlParameter("EndDate", EndDate)
                ).ToList();

            //combine rows with the same PayrollPaymentIds. duplicates caused by session with multiple students
            List<PayrollPaymentItemsModel> combinedModels = new List<PayrollPaymentItemsModel>();
            PayrollPaymentItemsModel combinedModel;
            foreach (PayrollPaymentItemsModel model in models)
            {
                combinedModel = combinedModels.Find(x => x.Id == model.Id);
                if (combinedModel == null)
                {
                    if (!string.IsNullOrEmpty(model.Student_UserAccounts_FirstName))
                        model.Description = Util.append(model.Description, model.Student_UserAccounts_FirstName, ",");

                    combinedModels.Add(model);
                }
                else
                {
                    combinedModel.Description = Util.append(combinedModel.Description, model.Student_UserAccounts_FirstName, ",");
                }
            }

            return combinedModels;
        }

    }
}