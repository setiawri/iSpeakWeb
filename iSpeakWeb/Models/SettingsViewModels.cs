﻿using System;
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
        public string AutoEntryForCashPayments_Notes { get; set; }

        [Display(Name = "User Set Roles Access when Disabled shows only this role")]
        public Guid UserSetRoleAllowed { get; set; }
        public string UserSetRoleAllowed_Notes { get; set; }

        [Display(Name = "Role Access for Reminders")]
        public List<string> RoleAccessForReminders { get; set; }
        public string RoleAccessForReminders_Notes { get; set; }

        [Display(Name = "Full Access for Tutor Schedules")]
        public List<string> FullAccessForTutorSchedules { get; set; }
        public string FullAccessForTutorSchedules_Notes { get; set; }

        [Display(Name = "Show Only Own User Data")]
        public List<string> ShowOnlyOwnUserData { get; set; }
        public string ShowOnlyOwnUserData_Notes { get; set; }

        [Display(Name = "Reset Password")]
        public string ResetPassword { get; set; }
        public string ResetPassword_Notes { get; set; }

        [Display(Name = "Fix Session Hours")]
        public string FixSessionHours { get; set; }
        public string FixSessionHours_Notes { get; set; }

        [Display(Name = "Payroll Rates Roles")]
        public List<string> PayrollRatesRoles { get; set; }
        public string PayrollRatesRoles_Notes { get; set; }
    }
}