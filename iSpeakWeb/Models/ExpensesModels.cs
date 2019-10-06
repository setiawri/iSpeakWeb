using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Expenses")]
    public class ExpensesModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        public DateTime Timestamp { get; set; }
        [Display(Name = "Expense Category")]
        public Guid ExpenseCategories_Id { get; set; }
        [Required]
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}