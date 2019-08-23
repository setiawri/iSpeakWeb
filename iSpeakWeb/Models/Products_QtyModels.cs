using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("Products_qty")]
    public class Products_QtyModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Branches_Id { get; set; }
        public Guid Products_Id { get; set; }
        public int Qty { get; set; }
    }
}