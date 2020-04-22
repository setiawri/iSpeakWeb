using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Settings")]
    public class SettingsModels
    {
        [Key]
        public Guid Id { get; set; }
        public int? Value_Int { get; set; }
        public string Value_String { get; set; }
        public Guid? Value_Guid { get; set; }
        public string Notes { get; set; }
    }

    public class SettingsValue
    {
        public static Guid GUID_AutoEntryForCashPayments = new Guid("5c62ee59-03a9-453a-95c9-a234f537adf1");
        public static Guid GUID_UserSetRoleAllowed = new Guid("25f53554-3b9d-4d3b-a8e5-58d921950987");
        public static Guid GUID_RoleAccessForReminders = new Guid("2f8d8208-4464-4a01-b9e1-30e54292b708");
        public static Guid GUID_FullAccessForTutorSchedule = new Guid("9b5ab31f-ce5e-4942-9e07-0fe107058910");
        public static Guid GUID_ResetPassword = new Guid("01f2d64d-f402-4a96-b854-128f9a9ae42f");
        public static Guid GUID_ShowOnlyOwnUserData = new Guid("70adb944-1917-4cd7-817d-6ca5fa789d5e");
        public static Guid GUID_FixSessionHours = new Guid("f647faa0-bab9-4e3a-9a25-42555a65ded5");
    }
}