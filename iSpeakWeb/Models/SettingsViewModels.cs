using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class SettingsViewModels
    {
        [Display(Name = "Auto Entry for Cash Payments")]
        public Guid AutoEntryForCashPayments { get; set; }
        [Display(Name = "User Set Role Allowed (No Access)")]
        public Guid UserSetRoleAllowed { get; set; }
    }
}