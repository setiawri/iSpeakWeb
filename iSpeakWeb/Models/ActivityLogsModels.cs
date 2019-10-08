using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("ActivityLogs")]
    public class ActivityLogsModels
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string TableName { get; set; }
        public Guid RefId { get; set; }
        public string Description { get; set; }
        //public string ColumnName { get; set; }
        //public string OriginalValue { get; set; }
        //public string NewValue { get; set; }
        public string UserAccounts_Id { get; set; }
    }
}