using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Logs")]
    public class LogsModels
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string TableName { get; set; }
        public Guid RefId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public string Description { get; set; }
        public string UserAccounts_Id { get; set; }
    }

    public class LogsViewModels
    {
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        [Display(Name = "User Input")]
        public string UserInput { get; set; }
        public string Description { get; set; }
    }
}