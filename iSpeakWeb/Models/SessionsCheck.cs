using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeakWeb.Models
{
    public class SessionsCheck
    {
        public string No { get; set; }
        public string Description { get; set; }
        public decimal SessionHours { get; set; }
        public decimal SessionHours_Remaining { get; set; }
        public decimal UsedHours { get; set; }
        public decimal TotalHours { get; set; }
    }
}