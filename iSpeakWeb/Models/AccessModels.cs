using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("RoleAccessMenu")]
    public class AccessModels
    {
        [Key]
        public Guid Id { get; set; }
        public string RoleId { get; set; }
        public string WebMenuAccess { get; set; }
    }

    public class AccessViewModels
    {
        [Display(Name = "#")]
        public int MenuOrder { get; set; }

        [Display(Name = "Menu")]
        public string ParentMenu { get; set; }

        [Display(Name = "Action Menu")]
        public string MenuName { get; set; }

        public string Notes { get; set; }

        [Display(Name = "Controller Action Reff.")]
        public string WebMenuAccess { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        public bool IsSelected { get; set; }
    }
}