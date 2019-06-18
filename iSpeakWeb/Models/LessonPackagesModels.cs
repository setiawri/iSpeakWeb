using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("LessonPackages")]
    public class LessonPackagesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Language")]
        public Guid Languages_Id { get; set; }
        [Display(Name = "Lesson Type")]
        public Guid LessonTypes_Id { get; set; }
        [Display(Name = "Session Hours")]
        public decimal SessionHours { get; set; }
        [Display(Name = "Expiration Day")]
        public int ExpirationDay { get; set; }
        public int Price { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }

    public class LessonPackagesViewModels
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Languages { get; set; }
        [Display(Name = "Lesson Type")]
        public string LessonTypes { get; set; }
        [Display(Name = "Hours")]
        public decimal SessionHours { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Price { get; set; }
        public bool Active { get; set; }
    }
}