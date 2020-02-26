using iSpeak.Models;
using iSpeak.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace iSpeak.Controllers.API
{
    public class SessionsController : ApiController
    {
        private readonly iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/session")]
        public HttpResponseMessage Sessions(CommonRequestModels model)
        {
            var roles = (from u in db.User
                         join ur in db.UserRole on u.Id equals ur.UserId
                         join r in db.Role on ur.RoleId equals r.Id
                         where u.UserName == model.Username
                         select new { u, r }).ToList();
            string role = "";
            if (roles.Count == 1)
            {
                foreach(var r in roles)
                {
                    if (r.r.Name.ToLower() == "student" || r.r.Name.ToLower() == "tutor")
                    {
                        role = r.r.Name;
                    }
                }
            }

            var sessions =
                (role.ToLower() == "student")
                ? (from ls in db.LessonSessions
                   join t in db.User on ls.Tutor_UserAccounts_Id equals t.Id
                   join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                   join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                   join s in db.User on si.Customer_UserAccounts_Id equals s.Id
                   join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                   where s.UserName == model.Username
                   select new { ls, t, sii, si, s, lp }).ToList()
                : (role.ToLower() == "tutor")
                    ? (from ls in db.LessonSessions
                       join t in db.User on ls.Tutor_UserAccounts_Id equals t.Id
                       join sii in db.SaleInvoiceItems on ls.SaleInvoiceItems_Id equals sii.Id
                       join si in db.SaleInvoices on sii.SaleInvoices_Id equals si.Id
                       join s in db.User on si.Customer_UserAccounts_Id equals s.Id
                       join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                       where t.UserName == model.Username
                       select new { ls, t, sii, si, s, lp }).ToList()
                   : null;
            List<SessionApiModels> list = new List<SessionApiModels>();
            if (sessions.Count > 0)
            {
                foreach (var session in sessions.OrderByDescending(x => x.ls.Timestamp))
                {
                    list.Add(new SessionApiModels
                    {
                        SaleInvoiceItems_Id = session.sii.Id,
                        Date = string.Format("{0:yyy/MM/dd HH:mm}", session.ls.Timestamp),
                        Lesson = session.lp.Name,
                        Hour = session.ls.SessionHours,
                        Tutor = session.t.Firstname + " " + session.t.Middlename + " " + session.t.Lastname,
                        Student = session.s.Firstname + " " + session.s.Middlename + " " + session.s.Lastname,
                        Review = session.ls.Review
                    });
                }
            }

            if (list == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }
    }
}
