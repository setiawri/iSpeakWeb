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

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //var login_session = HttpContext.Current.Session["Login"] as LoginViewModel;
            //filterContext.Controller.ViewBag.LoginBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
        }
    }
}