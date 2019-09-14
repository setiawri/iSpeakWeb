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
        private iSpeakContext db = new iSpeakContext();
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var settings_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
            var settings_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
            SettingsViewModels settingsViewModels = new SettingsViewModels()
            {
                AutoEntryForCashPayments = settings_aefcp.Value_Guid ?? Guid.Empty,
                UserSetRoleAllowed = settings_usra.Value_Guid ?? Guid.Empty
            };

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }

        [HttpPost]
        public async Task<ActionResult> Index([Bind(Include = "AutoEntryForCashPayments,UserSetRoleAllowed")] SettingsViewModels settingsViewModels)
        {
            if (settingsViewModels.AutoEntryForCashPayments == Guid.Empty || settingsViewModels.AutoEntryForCashPayments == null)
            {
                ModelState.AddModelError("AutoEntryForCashPayments", "The field Auto Entry for Cash Payments is required.");
            }

            if (settingsViewModels.UserSetRoleAllowed == Guid.Empty || settingsViewModels.UserSetRoleAllowed == null)
            {
                ModelState.AddModelError("UserSetRoleAllowed", "The field User Set Role Allowed is required.");
            }

            if (ModelState.IsValid)
            {
                SettingsModels settingsModels_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
                settingsModels_aefcp.Value_Guid = settingsViewModels.AutoEntryForCashPayments;
                db.Entry(settingsModels_aefcp).State = EntityState.Modified;

                SettingsModels settingsModels_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
                settingsModels_usra.Value_Guid = settingsViewModels.UserSetRoleAllowed;
                db.Entry(settingsModels_usra).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }
    }
}