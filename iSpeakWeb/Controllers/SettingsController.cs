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
            SettingsViewModels settingsViewModels = new SettingsViewModels()
            {
                AutoEntryForCashPayments = settings_aefcp.Value_Guid ?? Guid.Empty
            };

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }

        [HttpPost]
        public async Task<ActionResult> Index([Bind(Include = "AutoEntryForCashPayments")] SettingsViewModels settingsViewModels)
        {
            if (settingsViewModels.AutoEntryForCashPayments == Guid.Empty || settingsViewModels.AutoEntryForCashPayments == null)
            {
                ModelState.AddModelError("AutoEntryForCashPayments", "The field Auto Entry for Cash Payments is required.");
            }

            if (ModelState.IsValid)
            {
                SettingsModels settingsModels_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
                settingsModels_aefcp.Value_Guid = settingsViewModels.AutoEntryForCashPayments;
                db.Entry(settingsModels_aefcp).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(settingsViewModels);
        }
    }
}