using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class SaleInvoicesViewModels
    {
        [Display(Name = "Branch")]
        public Guid Branches_Id { get; set; }
        [Display(Name = "Lesson Package")]
        public Guid LessonPackages_Id { get; set; }
        [Display(Name = "Voucher")]
        public Guid? Vouchers_Id { get; set; }
        [Display(Name = "Customer")]
        public Guid Customers_Id { get; set; }
        public string Notes { get; set; }
    }
}