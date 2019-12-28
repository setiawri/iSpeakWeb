using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace iSpeak.Models
{
    public class iSpeakContext : DbContext
    {
        public DbSet<LogsModels> Logs { get; set; }
        public DbSet<ActivityLogsModels> ActivityLogs { get; set; }
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
        public DbSet<PettyCashRecordsCategoriesModels> PettyCashRecordsCategories { get; set; }
        public DbSet<PettyCashRecordsModels> PettyCashRecords { get; set; }
        public DbSet<ServicesModels> Services { get; set; }
        public DbSet<SettingsModels> Settings { get; set; }
        public DbSet<PayrollPaymentsModels> PayrollPayments { get; set; }
        public DbSet<PayrollPaymentItemsModels> PayrollPaymentItems { get; set; }
        public DbSet<SaleInvoiceItems_VouchersModels> SaleInvoiceItems_Vouchers { get; set; }
        public DbSet<RemindersModels> Reminders { get; set; }
        public DbSet<UploadFilesModels> UploadFiles { get; set; }
        public DbSet<ExpenseCategoriesModels> ExpenseCategories { get; set; }
        public DbSet<ExpensesModels> Expenses { get; set; }
        public DbSet<PromotionEventsModels> PromotionEvents { get; set; }
        public DbSet<SaleReturnsModels> SaleReturns { get; set; }
        public DbSet<SaleReturnItemsModels> SaleReturnItems { get; set; }
        public DbSet<TutorSchedulesModels> TutorSchedules { get; set; }
        public DbSet<TutorStudentSchedulesModels> TutorStudentSchedules { get; set; }


        public override int SaveChanges()
        {
            iSpeakContext db = new iSpeakContext();
            var user_model = db.User.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
            string userId = user_model == null ? "" : user_model.Id;
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (LogsModels x in GetAuditRecordsForChange(ent, userId))
                {
                    this.Logs.Add(x);
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {
            iSpeakContext db = new iSpeakContext();
            var user_model = db.User.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
            string userId = user_model == null ? "" : user_model.Id;
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (LogsModels x in GetAuditRecordsForChange(ent, userId))
                {
                    this.Logs.Add(x);
                }
            }

            return await base.SaveChangesAsync();
        }

        private List<LogsModels> GetAuditRecordsForChange(DbEntityEntry dbEntry, string userId)
        {
            List<LogsModels> result = new List<LogsModels>();

            DateTime changeTime = DateTime.UtcNow;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            string keyName = dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

            if (tableName != "Logs" && tableName != "RoleAccessMenu")
            {

                if (dbEntry.State == EntityState.Added)
                {
                    result.Add(new LogsModels()
                    {
                        Id = Guid.NewGuid(),
                        Action = "Added", // Added
                        TableName = tableName,
                        RefId = new Guid(dbEntry.CurrentValues.GetValue<object>(keyName).ToString()),
                        ColumnName = "*ALL",
                        //NewValue = dbEntry.CurrentValues.ToObject().ToString(), //(dbEntry.OriginalValues.ToObject() is IDescribableEntity) ? (dbEntry.OriginalValues.ToObject() as IDescribableEntity).Describe() : dbEntry.OriginalValues.ToObject().ToString(),
                        UserAccounts_Id = userId,
                        Timestamp = changeTime
                    });
                }
                else if (dbEntry.State == EntityState.Deleted)
                {
                    result.Add(new LogsModels()
                    {
                        Id = Guid.NewGuid(),
                        Action = "Deleted", // Deleted
                        TableName = tableName,
                        RefId = new Guid(dbEntry.OriginalValues.GetValue<object>(keyName).ToString()),
                        ColumnName = "*ALL",
                        UserAccounts_Id = userId,
                        Timestamp = changeTime
                    });
                }
                else if (dbEntry.State == EntityState.Modified)
                {
                    foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
                    {
                        // For updates, we only want to capture the columns that actually changed
                        if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                        {
                            result.Add(new LogsModels()
                            {
                                Id = Guid.NewGuid(),
                                Action = "Modified", // Modified
                                TableName = tableName,
                                RefId = new Guid(dbEntry.OriginalValues.GetValue<object>(keyName).ToString()),
                                ColumnName = propertyName,
                                OriginalValue = dbEntry.OriginalValues.GetValue<object>(propertyName)?.ToString(),
                                NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName)?.ToString(),
                                UserAccounts_Id = userId,
                                Timestamp = changeTime
                            });
                        }
                    }
                }
                // Otherwise, don't do anything, we don't care about Unchanged or Detached entities
            }
            return result;
        }
    }
}