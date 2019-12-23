using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("SaleReturns")]
    public class SaleReturnsModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string No { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        [Required]
        public string Notes { get; set; }
        public bool Approved { get; set; }
    }
}