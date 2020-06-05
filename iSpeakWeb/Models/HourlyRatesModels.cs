using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("HourlyRates")]
    public class HourlyRatesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Branch")]
        public Guid? Branches_Id { get; set; }
        [Display(Name = "Lesson Package")]
        public Guid? LessonPackages_Id { get; set; }
        [Required]
        [Display(Name = "Employee")]
        public string UserAccounts_Id { get; set; }
        public decimal Rate { get; set; }
        [Display(Name = "Full Time Payrate")]
        public decimal FullTimeTutorPayrate { get; set; }
        public string Notes { get; set; }
    }

    public class HourlyRatesViewModels
    {
        public Guid Id { get; set; }
        public string Branch { get; set; }
        [Display(Name = "Lesson Package")]
        public string LessonPackages { get; set; }
        [Display(Name = "Employee")]
        public string UserAccounts { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Rate { get; set; }
        [Display(Name = "Full Time Payrate")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FullTimeTutorPayrate { get; set; }
        public string Notes { get; set; }
    }
}