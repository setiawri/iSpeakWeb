using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var settings_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
            var settings_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
            var settings_rafr = await db.Settings.FindAsync(SettingsValue.GUID_RoleAccessForReminders);
            var settings_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);

            List<string> role_for_reminder = new List<string>();
            if (!string.IsNullOrEmpty(settings_rafr.Value_String))
            {
                string[] ids = settings_rafr.Value_String.Split(',');
                foreach (var id in ids)
                {
                    role_for_reminder.Add(id);
                }
            }

            List<string> role_for_tutor_schedule = new List<string>();
            if (!string.IsNullOrEmpty(settings_fafts.Value_String))
            {
                string[] ids = settings_fafts.Value_String.Split(',');
                foreach (var id in ids)
                {
                    role_for_tutor_schedule.Add(id);
                }
            }

            SettingsViewModels settingsViewModels = new SettingsViewModels()
            {
                AutoEntryForCashPayments = settings_aefcp.Value_Guid ?? Guid.Empty,
                UserSetRoleAllowed = settings_usra.Value_Guid ?? Guid.Empty,
                RoleAccessForReminders = role_for_reminder,
                FullAccessForTutorSchedules = role_for_tutor_schedule
            };

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }

        [HttpPost]
        public async Task<ActionResult> Index([Bind(Include = "AutoEntryForCashPayments,UserSetRoleAllowed,RoleAccessForReminders,FullAccessForTutorSchedules")] SettingsViewModels settingsViewModels)
        {
            if (settingsViewModels.AutoEntryForCashPayments == Guid.Empty || settingsViewModels.AutoEntryForCashPayments == null)
            {
                ModelState.AddModelError("AutoEntryForCashPayments", "The field Auto Entry for Cash Payments is required.");
            }

            if (settingsViewModels.UserSetRoleAllowed == Guid.Empty || settingsViewModels.UserSetRoleAllowed == null)
            {
                ModelState.AddModelError("UserSetRoleAllowed", "The field User Set Role Allowed is required.");
            }

            if (settingsViewModels.RoleAccessForReminders == null)
            {
                ModelState.AddModelError("RoleAccessForReminders", "The field Role for Reminders is required.");
            }

            if (settingsViewModels.FullAccessForTutorSchedules == null)
            {
                ModelState.AddModelError("FullAccessForTutorSchedules", "The field Role for Tutor Schedule is required.");
            }

            if (ModelState.IsValid)
            {
                SettingsModels settingsModels_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
                settingsModels_aefcp.Value_Guid = settingsViewModels.AutoEntryForCashPayments;
                db.Entry(settingsModels_aefcp).State = EntityState.Modified;

                SettingsModels settingsModels_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
                settingsModels_usra.Value_Guid = settingsViewModels.UserSetRoleAllowed;
                db.Entry(settingsModels_usra).State = EntityState.Modified;

                SettingsModels settingsModels_rafr = await db.Settings.FindAsync(SettingsValue.GUID_RoleAccessForReminders);
                settingsModels_rafr.Value_String = string.Join(",", settingsViewModels.RoleAccessForReminders);
                db.Entry(settingsModels_rafr).State = EntityState.Modified;

                SettingsModels settingsModels_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                settingsModels_fafts.Value_String = string.Join(",", settingsViewModels.FullAccessForTutorSchedules);
                db.Entry(settingsModels_fafts).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }
    }
}