using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    [Table("AspNetUsers")]
    public class UserModels
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public DateTime Birthday { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }

    [Table("AspNetRoles")]
    public class RoleModels
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [Table("AspNetUserRoles")]
    public class UserRoleModels
    {
        [Key]
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}