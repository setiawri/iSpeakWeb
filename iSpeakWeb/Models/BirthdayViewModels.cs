using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class BirthdayViewModels
    {
        public string Fullname { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime Birthday { get; set; }
        public string Role { get; set; }
        public int CountDay { get; set; }
    }

    public class BirthdayHome
    {
        public List<RemindersModels> Reminders { get; set; }
        public List<BirthdayViewModels> ThisMonth { get; set; }
        public List<BirthdayViewModels> NextMonth { get; set; }
        public bool IsStudentBirthday { get; set; }
        public bool ShowBirthdayList { get; set; }
        public bool IsRemindersAllowed { get; set; }
    }
}