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

            bool isSettings = p.IsGranted(filterContext.HttpContext.User.Identity.Name, "navmenu_settings");
            filterContext.Controller.ViewBag.SettingsMenu = (isSettings) ? "" : "style='display: none'";

        }
    }
}