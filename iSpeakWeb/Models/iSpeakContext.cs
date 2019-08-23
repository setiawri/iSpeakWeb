using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace iSpeak.Models
{
    public class iSpeakContext : DbContext
    {
        public DbSet<MasterMenuModels> MasterMenu { get; set; }
        public DbSet<AccessModels> Access { get; set; }
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
        public DbSet<SaleInvoiceItemsModels> SaleInvoiceItems { get; set; }
        public DbSet<PaymentsModels> Payments { get; set; }
        public DbSet<PaymentItemsModels> PaymentItems { get; set; }
        public DbSet<LessonSessionsModels> LessonSessions { get; set; }
        public DbSet<ProductsModels> Products { get; set; }
        public DbSet<InventoryModels> Inventory { get; set; }
        public DbSet<SaleInvoiceItems_InventoryModels> SaleInvoiceItems_Inventory { get; set; }
        public DbSet<ConsignmentsModels> Consignments { get; set; }
        public DbSet<Products_QtyModels> Products_Qty { get; set; }
    }
}