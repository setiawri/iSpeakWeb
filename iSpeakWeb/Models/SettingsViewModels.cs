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
        [Display(Name = "User Set Roles Access when Disabled shows only this role")]
        public Guid UserSetRoleAllowed { get; set; }
        [Display(Name = "Role Access for Reminders")]
        public List<string> RoleAccessForReminders { get; set; }
        [Display(Name = "Full Access for Tutor Schedules")]
        public List<string> FullAccessForTutorSchedules { get; set; }
    }
}