using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class iSpeakContext : DbContext
    {
        public DbSet<UserModels> User { get; set; }
        public DbSet<RoleModels> Role { get; set; }
        public DbSet<UserRoleModels> UserRole { get; set; }
    }
}