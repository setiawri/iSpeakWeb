using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class PettyCashViewModels
    {
        public Guid Id { get; set; }
        public string No { get; set; }
        [Display(Name = "Date")]
        public string Timestamp { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Amount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Balance { get; set; }
        [Display(Name = "Status")]
        public bool IsChecked { get; set; }
        [Display(Name = "Created By")]
        public string UserInput { get; set; }
        public string Status_render { get; set; }
        public string Action_render { get; set; }
    }
}