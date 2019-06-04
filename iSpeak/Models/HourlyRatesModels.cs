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
        [Display(Name = "Lesson Package")]
        public Guid LessonPackages_Id { get; set; }
        [Required]
        [Display(Name = "User")]
        public string UserAccounts_Id { get; set; }
        public decimal Rate { get; set; }
        public string Notes { get; set; }
    }

    public class HourlyRatesViewModels
    {
        public Guid Id { get; set; }
        [Display(Name = "Lesson Package")]
        public string LessonPackages { get; set; }
        [Display(Name = "User")]
        public string UserAccounts { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Rate { get; set; }
        public string Notes { get; set; }
    }
}