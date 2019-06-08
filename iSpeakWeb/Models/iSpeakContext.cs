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
        public DbSet<BranchesModels> Branches { get; set; }
        public DbSet<LanguagesModels> Languages { get; set; }
        public DbSet<LessonTypesModels> LessonTypes { get; set; }
        public DbSet<LessonPackagesModels> LessonPackages { get; set; }
        public DbSet<HourlyRatesModels> HourlyRates { get; set; }
        public DbSet<UnitsModels> Units { get; set; }
        public DbSet<SuppliersModels> Suppliers { get; set; }
        public DbSet<VouchersModels> Vouchers { get; set; }
        public DbSet<SaleInvoicesModels> SaleInvoices { get; set; }
    }
}