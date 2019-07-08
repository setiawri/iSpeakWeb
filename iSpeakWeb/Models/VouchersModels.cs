using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Vouchers")]
    public class VouchersModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Description { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }
}