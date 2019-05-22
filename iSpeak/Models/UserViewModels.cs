using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class UserViewModels
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string RoleId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Full Name")]
        public string Fullname { get; set; }

        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime Birthday { get; set; }

        public string Notes { get; set; }
        public bool Active { get; set; }
    }
}