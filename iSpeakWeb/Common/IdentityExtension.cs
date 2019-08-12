using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace iSpeak.Common
{
    public static class IdentityExtension
    {
        public static string GetFullName(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            ClaimsIdentity ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("Fullname");
            }
            return null;
        }

        public static string GetBranches_Id(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            ClaimsIdentity ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("Branches_Id");
            }
            return null;
        }
    }
}