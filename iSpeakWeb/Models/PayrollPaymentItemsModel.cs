using System;
using System.ComponentModel.DataAnnotations;

namespace iSpeak.Models
{
    public class PayrollPaymentItemsModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? PayrollPayments_Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? Timestamp { get; set; }

        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Hour { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal HourlyRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TutorTravelCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

        public string UserAccounts_Id { get; set; }
        public string CancelNotes { get; set; }
        public Guid? Branches_Id { get; set; }


        public string Tutor_UserAccounts_FullName { get; set; }
        public string Student_UserAccounts_FirstName { get; set; }
    }
}