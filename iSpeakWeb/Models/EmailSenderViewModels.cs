using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class EmailSenderViewModels
    {
        [Display(Name = "Interest Language")]
        public string InterestLanguage { get; set; }
        [Required]
        [EmailAddress]
        public string From { get; set; }
        public string To { get; set; }
        [Required]
        public string Subject { get; set; }
        public string Footer { get; set; }
        [Required]
        public string Password { get; set; }
    }
}