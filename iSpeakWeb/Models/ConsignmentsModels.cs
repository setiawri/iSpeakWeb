using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Consignments")]
    public class ConsignmentsModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Branch")]
        public Guid Branches_Id { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }
}