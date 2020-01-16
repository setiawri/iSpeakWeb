using System;
using System.Collections.Generic;
using System.Text;

namespace iSpeakMobile.Models
{
    public static class DataStore
    {
        public static IList<Invoice> Invoices { get; set; }
        public static IList<Payment> Payments { get; set; }
        public static IList<Session> Sessions { get; set; }
        public static IList<Schedule> Schedules { get; set; }
        public static IList<TutorSchedule> TutorSchedules { get; set; }
    }
}
