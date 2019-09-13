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
        private iSpeakContext db = new iSpeakContext();
        private Permission p = new Permission();

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //var login_session = HttpContext.Current.Session["Login"] as LoginViewModel;
            filterContext.Controller.ViewBag.LoginBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");

            bool isSale = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_saleinvoice");
            filterContext.Controller.ViewBag.SaleMenu = (isSale) ? "" : "style='display: none'";

            bool isPayment = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_payment");
            filterContext.Controller.ViewBag.PaymentMenu = (isPayment) ? "" : "style='display: none'";

            bool isSessions = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_sessions");
            filterContext.Controller.ViewBag.SessionsMenu = (isSessions) ? "" : "style='display: none'";

            bool isInventory = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_inventory");
            filterContext.Controller.ViewBag.InventoryMenu = (isInventory) ? "" : "style='display: none'";

            bool isPettyCash = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_pettycash");
            filterContext.Controller.ViewBag.PettyCashMenu = (isPettyCash) ? "" : "style='display: none'";

            bool isReports = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_reports");
            filterContext.Controller.ViewBag.ReportsMenu = (isReports) ? "" : "style='display: none'";

            bool isMasterData = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_masterdata");
            filterContext.Controller.ViewBag.MasterDataMenu = (isMasterData) ? "" : "style='display: none'";

            bool isAccounts = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_accounts");
            filterContext.Controller.ViewBag.AccountsMenu = (isAccounts) ? "" : "style='display: none'";
        }
    }
}