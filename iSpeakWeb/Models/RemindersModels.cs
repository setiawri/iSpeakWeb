using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Reminders")]
    public class RemindersModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime Timestamp { get; set; }
        [Required]
        public string Description { get; set; }
        public RemindersStatusEnum Status_enumid { get; set; }
    }
}