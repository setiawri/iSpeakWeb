using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Common
{
    public class BranchesActionFilter : ActionFilterAttribute
    {
        //private iSpeakContext db = new iSpeakContext();
        private Permission p = new Permission();

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //no-cache
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache); // HTTP 1.1.
            filterContext.HttpContext.Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            filterContext.HttpContext.Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            filterContext.HttpContext.Response.AppendHeader("Expires", "0"); // Proxies.

            string userlogin = filterContext.HttpContext.User.Identity.Name;
            using (var db = new iSpeakContext())
            {
                filterContext.Controller.ViewBag.LoginBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

                //bool isSale = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_saleinvoice");
                //filterContext.Controller.ViewBag.SaleMenu = (isSale) ? "" : "style='display: none'";

                //bool isSchedule = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_schedules");
                //filterContext.Controller.ViewBag.ScheduleMenu = (isSchedule) ? "" : "style='display: none'";

                //bool isSessions = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_sessions");
                //filterContext.Controller.ViewBag.SessionsMenu = (isSessions) ? "" : "style='display: none'";

                //bool isInventory = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_inventory");
                //filterContext.Controller.ViewBag.InventoryMenu = (isInventory) ? "" : "style='display: none'";

                //bool isPettyCash = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_pettycash");
                //filterContext.Controller.ViewBag.PettyCashMenu = (isPettyCash) ? "" : "style='display: none'";

                //bool isReports = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_reports");
                //filterContext.Controller.ViewBag.ReportsMenu = (isReports) ? "" : "style='display: none'";

                //bool isMasterData = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_masterdata");
                //filterContext.Controller.ViewBag.MasterDataMenu = (isMasterData) ? "" : "style='display: none'";

                //bool isAccounts = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_accounts");
                //filterContext.Controller.ViewBag.AccountsMenu = (isAccounts) ? "" : "style='display: none'";

                //bool isFiles = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_files");
                //filterContext.Controller.ViewBag.FilesMenu = (isFiles) ? "" : "style='display: none'";

                #region SALE
                #region Sale Invoices
                filterContext.Controller.ViewBag.SaleIndex = p.IsGranted(userlogin, "sale_index");
                filterContext.Controller.ViewBag.SaleCreate = p.IsGranted(userlogin, "sale_create");
                #endregion
                #region Payments
                filterContext.Controller.ViewBag.PaymentsPrint = p.IsGranted(userlogin, "payments_print");
                filterContext.Controller.ViewBag.PaymentsCreate = p.IsGranted(userlogin, "payments_create");
                #endregion
                #region Returns
                filterContext.Controller.ViewBag.ReturnIndex = p.IsGranted(userlogin, "return_index");
                filterContext.Controller.ViewBag.ReturnCreate = p.IsGranted(userlogin, "return_create");
                #endregion
                if (filterContext.Controller.ViewBag.SaleIndex == true || filterContext.Controller.ViewBag.PaymentsPrint == true || filterContext.Controller.ViewBag.ReturnIndex == true)
                {
                    filterContext.Controller.ViewBag.SaleMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.SaleMenu = "style='display: none'";
                }
                #endregion
                #region SCHEDULES
                #region Tutor
                filterContext.Controller.ViewBag.SchedulesTutorIndex = p.IsGranted(userlogin, "schedules_tutorindex");
                filterContext.Controller.ViewBag.SchedulesTutorCreate = p.IsGranted(userlogin, "schedules_tutorcreate");
                filterContext.Controller.ViewBag.SchedulesTutorEdit = p.IsGranted(userlogin, "schedules_tutoredit");
                filterContext.Controller.ViewBag.SchedulesTutorDelete = p.IsGranted(userlogin, "schedules_tutordelete");
                #endregion
                #region Student
                filterContext.Controller.ViewBag.SchedulesStudentIndex = p.IsGranted(userlogin, "schedules_studentindex");
                filterContext.Controller.ViewBag.SchedulesStudentCreate = p.IsGranted(userlogin, "schedules_studentcreate");
                filterContext.Controller.ViewBag.SchedulesStudentEdit = p.IsGranted(userlogin, "schedules_studentedit");
                filterContext.Controller.ViewBag.SchedulesStudentDelete = p.IsGranted(userlogin, "schedules_studentdelete");
                #endregion
                #region Search
                filterContext.Controller.ViewBag.SchedulesSearch = p.IsGranted(userlogin, "schedules_search");
                #endregion
                if (filterContext.Controller.ViewBag.SchedulesTutorIndex == true || filterContext.Controller.ViewBag.SchedulesStudentIndex == true || filterContext.Controller.ViewBag.SchedulesSearch == true)
                {
                    filterContext.Controller.ViewBag.ScheduleMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.ScheduleMenu = "style='display: none'";
                }
                #endregion
                #region SESSIONS
                filterContext.Controller.ViewBag.SessionsMenu = (p.IsGranted(userlogin, "lessonsessions_index")) ? "" : "style='display: none'";
                filterContext.Controller.ViewBag.LessonSessionsCreate = p.IsGranted(userlogin, "lessonsessions_create");
                filterContext.Controller.ViewBag.LessonSessionsEdit = p.IsGranted(userlogin, "lessonsessions_edit");
                #endregion
                #region INVENTORY
                filterContext.Controller.ViewBag.InventoryMenu = (p.IsGranted(userlogin, "inventory_index")) ? "" : "style='display: none'";
                filterContext.Controller.ViewBag.InventoryCreate = p.IsGranted(userlogin, "inventory_create");
                filterContext.Controller.ViewBag.InventoryEdit = p.IsGranted(userlogin, "inventory_edit");
                filterContext.Controller.ViewBag.InventoryStock = p.IsGranted(userlogin, "inventory_stock");
                #endregion
                #region PETTY CASH
                filterContext.Controller.ViewBag.PettyCashMenu = (p.IsGranted(userlogin, "pettycashrecords_index")) ? "" : "style='display: none'";
                filterContext.Controller.ViewBag.PettyCashRecordsCreate = p.IsGranted(userlogin, "pettycashrecords_create");
                #endregion
                #region INTERNAL
                #region Hourly Rates
                filterContext.Controller.ViewBag.HourlyRatesIndex = p.IsGranted(userlogin, "hourlyrates_index");
                filterContext.Controller.ViewBag.HourlyRatesCreate = p.IsGranted(userlogin, "hourlyrates_create");
                filterContext.Controller.ViewBag.HourlyRatesEdit = p.IsGranted(userlogin, "hourlyrates_edit");
                filterContext.Controller.ViewBag.HourlyRatesDelete = p.IsGranted(userlogin, "hourlyrates_delete");
                #endregion
                filterContext.Controller.ViewBag.ReportsNewStudent = p.IsGranted(userlogin, "reports_newstudent");
                filterContext.Controller.ViewBag.ReportsPayroll = p.IsGranted(userlogin, "reports_payroll");
                filterContext.Controller.ViewBag.ReportsPayrollPayments = p.IsGranted(userlogin, "reports_payrollpayments");
                filterContext.Controller.ViewBag.ReportsProfitLoss = p.IsGranted(userlogin, "reports_profitloss");
                filterContext.Controller.ViewBag.ReportsSendEmails = p.IsGranted(userlogin, "reports_sendemails");

                if (filterContext.Controller.ViewBag.ReportsNewStudent == true || filterContext.Controller.ViewBag.ReportsPayroll == true || filterContext.Controller.ViewBag.ReportsPayrollPayments == true || filterContext.Controller.ViewBag.ReportsProfitLoss == true || filterContext.Controller.ViewBag.ReportsSendEmails == true || filterContext.Controller.ViewBag.HourlyRatesIndex == true)
                {
                    filterContext.Controller.ViewBag.InternalMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.InternalMenu = "style='display: none'";
                }
                #endregion
                #region MASTER DATA
                #region Inventory
                #region Suppliers
                filterContext.Controller.ViewBag.SuppliersIndex = p.IsGranted(userlogin, "suppliers_index");
                filterContext.Controller.ViewBag.SuppliersCreate = p.IsGranted(userlogin, "suppliers_create");
                filterContext.Controller.ViewBag.SuppliersEdit = p.IsGranted(userlogin, "suppliers_edit");
                #endregion
                #region Products
                filterContext.Controller.ViewBag.ProductsIndex = p.IsGranted(userlogin, "products_index");
                filterContext.Controller.ViewBag.ProductsCreate = p.IsGranted(userlogin, "products_create");
                filterContext.Controller.ViewBag.ProductsEdit = p.IsGranted(userlogin, "products_edit");
                #endregion
                #region Units
                filterContext.Controller.ViewBag.UnitsIndex = p.IsGranted(userlogin, "units_index");
                filterContext.Controller.ViewBag.UnitsCreate = p.IsGranted(userlogin, "units_create");
                filterContext.Controller.ViewBag.UnitsEdit = p.IsGranted(userlogin, "units_edit");
                #endregion

                if (filterContext.Controller.ViewBag.SuppliersIndex == true || filterContext.Controller.ViewBag.ProductsIndex == true || filterContext.Controller.ViewBag.UnitsIndex == true)
                {
                    filterContext.Controller.ViewBag.MasterData_InventoryMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.MasterData_InventoryMenu = "style='display: none'";
                }
                #endregion
                #region Lessons
                #region Languages
                filterContext.Controller.ViewBag.LanguagesIndex = p.IsGranted(userlogin, "languages_index");
                filterContext.Controller.ViewBag.LanguagesCreate = p.IsGranted(userlogin, "languages_create");
                filterContext.Controller.ViewBag.LanguagesEdit = p.IsGranted(userlogin, "languages_edit");
                #endregion
                #region Lesson Types
                filterContext.Controller.ViewBag.LessonTypesIndex = p.IsGranted(userlogin, "lessontypes_index");
                filterContext.Controller.ViewBag.LessonTypesCreate = p.IsGranted(userlogin, "lessontypes_create");
                filterContext.Controller.ViewBag.LessonTypesEdit = p.IsGranted(userlogin, "lessontypes_edit");
                #endregion
                #region LessonPackages
                filterContext.Controller.ViewBag.LessonPackagesIndex = p.IsGranted(userlogin, "lessonpackages_index");
                filterContext.Controller.ViewBag.LessonPackagesCreate = p.IsGranted(userlogin, "lessonpackages_create");
                filterContext.Controller.ViewBag.LessonPackagesEdit = p.IsGranted(userlogin, "lessonpackages_edit");
                #endregion
                
                if (filterContext.Controller.ViewBag.LanguagesIndex == true || filterContext.Controller.ViewBag.LessonTypesIndex == true || filterContext.Controller.ViewBag.LessonPackagesIndex == true)
                {
                    filterContext.Controller.ViewBag.MasterData_LessonsMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.MasterData_LessonsMenu = "style='display: none'";
                }
                #endregion
                #region Expenses Sub Menu
                #region Expenses Categories
                filterContext.Controller.ViewBag.ExpenseCategoriesIndex = p.IsGranted(userlogin, "expensecategories_index");
                filterContext.Controller.ViewBag.ExpenseCategoriesCreate = p.IsGranted(userlogin, "expensecategories_create");
                filterContext.Controller.ViewBag.ExpenseCategoriesEdit = p.IsGranted(userlogin, "expensecategories_edit");
                #endregion
                #region Expenses
                filterContext.Controller.ViewBag.ExpensesIndex = p.IsGranted(userlogin, "expenses_index");
                filterContext.Controller.ViewBag.ExpensesCreate = p.IsGranted(userlogin, "expenses_create");
                filterContext.Controller.ViewBag.ExpensesEdit = p.IsGranted(userlogin, "expenses_edit");
                #endregion

                if (filterContext.Controller.ViewBag.ExpenseCategoriesIndex == true || filterContext.Controller.ViewBag.ExpensesIndex == true)
                {
                    filterContext.Controller.ViewBag.MasterData_ExpensesMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.MasterData_ExpensesMenu = "style='display: none'";
                }
                #endregion
                #region General Settings
                filterContext.Controller.ViewBag.SettingsIndex = p.IsGranted(userlogin, "settings_index");
                #region Role
                filterContext.Controller.ViewBag.RoleIndex = p.IsGranted(userlogin, "role_index");
                filterContext.Controller.ViewBag.RoleCreate = p.IsGranted(userlogin, "role_create");
                filterContext.Controller.ViewBag.RoleEdit = p.IsGranted(userlogin, "role_edit");
                filterContext.Controller.ViewBag.RoleManage = p.IsGranted(userlogin, "role_manage");
                #endregion
                #region Branches
                filterContext.Controller.ViewBag.BranchesIndex = p.IsGranted(userlogin, "branches_index");
                filterContext.Controller.ViewBag.BranchesCreate = p.IsGranted(userlogin, "branches_create");
                filterContext.Controller.ViewBag.BranchesEdit = p.IsGranted(userlogin, "branches_edit");
                #endregion

                if (filterContext.Controller.ViewBag.SettingsIndex == true || filterContext.Controller.ViewBag.RoleIndex == true || filterContext.Controller.ViewBag.BranchesIndex == true)
                {
                    filterContext.Controller.ViewBag.MasterData_GeneralSettingsMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.MasterData_GeneralSettingsMenu = "style='display: none'";
                }
                #endregion
                #region Consignments
                filterContext.Controller.ViewBag.ConsignmentsIndex = p.IsGranted(userlogin, "consignments_index");
                filterContext.Controller.ViewBag.ConsignmentsCreate = p.IsGranted(userlogin, "consignments_create");
                filterContext.Controller.ViewBag.ConsignmentsEdit = p.IsGranted(userlogin, "consignments_edit");
                #endregion
                #region Vouchers
                filterContext.Controller.ViewBag.VouchersIndex = p.IsGranted(userlogin, "vouchers_index");
                filterContext.Controller.ViewBag.VouchersCreate = p.IsGranted(userlogin, "vouchers_create");
                filterContext.Controller.ViewBag.VouchersEdit = p.IsGranted(userlogin, "vouchers_edit");
                #endregion
                #region Services
                filterContext.Controller.ViewBag.ServicesIndex = p.IsGranted(userlogin, "services_index");
                filterContext.Controller.ViewBag.ServicesCreate = p.IsGranted(userlogin, "services_create");
                filterContext.Controller.ViewBag.ServicesEdit = p.IsGranted(userlogin, "services_edit");
                #endregion
                #region Petty Cash Categories
                filterContext.Controller.ViewBag.PettyCashRecordsCategoriesIndex = p.IsGranted(userlogin, "pettyCashrecordscategories_index");
                filterContext.Controller.ViewBag.PettyCashRecordsCategoriesCreate = p.IsGranted(userlogin, "pettyCashrecordscategories_create");
                filterContext.Controller.ViewBag.PettyCashRecordsCategoriesEdit = p.IsGranted(userlogin, "pettyCashrecordscategories_edit");
                #endregion
                #region Promotion Events
                filterContext.Controller.ViewBag.PromotionEventsIndex = p.IsGranted(userlogin, "promotionevents_index");
                filterContext.Controller.ViewBag.PromotionEventsCreate = p.IsGranted(userlogin, "promotionevents_create");
                filterContext.Controller.ViewBag.PromotionEventsEdit = p.IsGranted(userlogin, "promotionevents_edit");
                #endregion

                if (filterContext.Controller.ViewBag.MasterData_InventoryMenu == ""
                    || filterContext.Controller.ViewBag.MasterData_LessonsMenu == ""
                    || filterContext.Controller.ViewBag.MasterData_ExpensesMenu == ""
                    || filterContext.Controller.ViewBag.MasterData_GeneralSettingsMenu == ""
                    || filterContext.Controller.ViewBag.ConsignmentsIndex == true
                    || filterContext.Controller.ViewBag.VouchersIndex == true
                    || filterContext.Controller.ViewBag.ServicesIndex == true
                    || filterContext.Controller.ViewBag.PettyCashRecordsCategoriesIndex == true
                    || filterContext.Controller.ViewBag.PromotionEventsIndex == true)
                {
                    filterContext.Controller.ViewBag.MasterDataMenu = "";
                }
                else
                {
                    filterContext.Controller.ViewBag.MasterDataMenu = "style='display: none'";
                }
                #endregion
                #region ACCOUNTS
                filterContext.Controller.ViewBag.AccountsMenu = (p.IsGranted(userlogin, "user_index")) ? "" : "style='display: none'";
                filterContext.Controller.ViewBag.AccountRegister = p.IsGranted(userlogin, "account_register");
                filterContext.Controller.ViewBag.UserEdit = p.IsGranted(userlogin, "user_edit");
                filterContext.Controller.ViewBag.UserResetPassword = p.IsGranted(userlogin, "user_resetpassword");
                #endregion
                #region FILES
                filterContext.Controller.ViewBag.FilesMenu = (p.IsGranted(userlogin, "files_index")) ? "" : "style='display: none'";
                filterContext.Controller.ViewBag.FilesCreate = p.IsGranted(userlogin, "files_create");
                filterContext.Controller.ViewBag.FilesUpdate = p.IsGranted(userlogin, "files_update");
                #endregion
            }
        }
    }
}