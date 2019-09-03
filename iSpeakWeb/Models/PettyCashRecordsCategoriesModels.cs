using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PettyCashRecordsCategories")]
    public class PettyCashRecordsCategoriesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Default Row")]
        public bool Default_row { get; set; }
        public bool Active { get; set; }
    }
}