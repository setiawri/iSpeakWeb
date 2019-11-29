using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PromotionEvents")]
    public class PromotionEventsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
        [Display(Name = "Total Days")]
        public int TotalDays { get; set; }
        [Display(Name = "Event Fee")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int EventFee { get; set; }
        [Display(Name = "Personnel Cost")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PersonnelCost { get; set; }
        [Display(Name = "Add. Cost")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AdditionalCost { get; set; }
        public string Notes { get; set; }
    }
}