using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Common
{
    public class Permission
    {
        //private Models.iSpeakContext db = new Models.iSpeakContext();

        public bool IsGranted(string username, string controller_action)
        {
            using (var db = new Models.iSpeakContext())
            {
                var isValid = (from u in db.User
                               join ur in db.UserRole on u.Id equals ur.UserId
                               join a in db.Access on ur.RoleId equals a.RoleId
                               where u.UserName == username && a.WebMenuAccess == controller_action.ToLower()
                               select u);
                if (isValid.Count() > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}