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
    }

    public class SettingsValue
    {
        public static Guid GUID_AutoEntryForCashPayments = new Guid("5c62ee59-03a9-453a-95c9-a234f537adf1");
    }
}