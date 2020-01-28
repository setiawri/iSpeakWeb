using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("MasterMenu")]
    public class MasterMenuModels
    {
        [Key]
        public Guid Id { get; set; }
        public string ParentMenu { get; set; }
        public int ParentOrder { get; set; }
        public string MenuName { get; set; }
        public int MenuOrder { get; set; }
        public string WebMenuAccess { get; set; }
        public string Notes { get; set; }
    }
}