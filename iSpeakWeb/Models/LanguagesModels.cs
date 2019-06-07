using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Languages")]
    public class LanguagesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}