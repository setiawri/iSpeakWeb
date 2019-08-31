using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class TokenIdentity
    {
        public string access_token { get; set; }
        public string userName { get; set; }
        public string error_description { get; set; }
    }
}