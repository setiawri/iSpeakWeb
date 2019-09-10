using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Services")]
    public class ServicesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Display(Name = "Unit")]
        public Guid Units_Id { get; set; }
        [Display(Name = "For Sale")]
        public bool ForSale { get; set; }
        [Display(Name = "Sell Price")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int SellPrice { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }

    public class ServicesViewModels
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        [Display(Name = "For Sale")]
        public bool ForSale { get; set; }
        public string Unit { get; set; }
        [Display(Name = "Sell Price")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int SellPrice { get; set; }
        public bool Active { get; set; }
    }
}