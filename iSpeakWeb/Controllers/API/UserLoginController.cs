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
    public class UserLoginController : ApiController
    {
        private readonly iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/userlogin")]
        public HttpResponseMessage UserLogin(CommonRequestModels model)
        {
            var user_login = db.User.Where(x => x.UserName == model.Username).FirstOrDefault();
            var user_roles = (from ur in db.UserRole
                              join r in db.Role on ur.RoleId equals r.Id
                              where ur.UserId == user_login.Id
                              select new { ur, r }).ToList();
            string role = "";
            if (user_roles.Count == 1)
            {
                foreach (var r in user_roles)
                {
                    if (r.r.Name.ToLower() == "student" || r.r.Name.ToLower() == "tutor") 
                    {
                        role = r.r.Name; 
                    }
                }
            }

            UserApiModels userApiModels = new UserApiModels
            {
                Username = user_login.UserName,
                Fullname = user_login.Firstname + " " + user_login.Middlename + " " + user_login.Lastname,
                Email = user_login.Email,
                Role = role
            };
            return Request.CreateResponse(HttpStatusCode.OK, userApiModels);
        }
    }
}
