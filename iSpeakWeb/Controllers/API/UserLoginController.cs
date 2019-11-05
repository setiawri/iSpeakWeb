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
        private iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/userlogin")]
        public HttpResponseMessage UserLogin(CommonRequestModels model)
        {
            var user_login = db.User.Where(x => x.UserName == model.Username).FirstOrDefault();
            UserApiModels userApiModels = new UserApiModels
            {
                Username = user_login.UserName,
                Fullname = user_login.Firstname + " " + user_login.Middlename + " " + user_login.Lastname,
                Email = user_login.Email
            };
            return Request.CreateResponse(HttpStatusCode.OK, userApiModels);
        }
    }
}
