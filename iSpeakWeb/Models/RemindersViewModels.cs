using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class RemindersViewModels
    {
        public Guid Id { get; set; }
        public string Timestamp { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Action_Render { get; set; }
    }
}