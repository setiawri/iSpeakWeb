using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("PettyCashRecords")]
    public class PettyCashRecordsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        public Guid? RefId { get; set; }
        [Required]
        public string No { get; set; }
        public DateTime Timestamp { get; set; }
        [Display(Name = "Petty Cash Category")]
        public Guid PettyCashRecordsCategories_Id { get; set; }
        public string Notes { get; set; }
        public int Amount { get; set; }
        public bool IsChecked { get; set; }
        public string UserAccounts_Id { get; set; }
        [Display(Name = "Expense Category")]
        public Guid? ExpenseCategories_Id { get; set; }
    }
}