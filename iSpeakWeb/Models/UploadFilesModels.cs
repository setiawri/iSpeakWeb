using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("UploadFiles")]
    public class UploadFilesModels
    {
        [Key]
        public Guid Id { get; set; }
        [Display(Name = "Branch")]
        public Guid? Branches_Id { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public string Filename { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsOlder { get; set; }
    }
}