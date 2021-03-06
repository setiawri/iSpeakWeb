﻿using iSpeak.Common;
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
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var settings_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
                var settings_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
                var settings_rafr = await db.Settings.FindAsync(SettingsValue.GUID_RoleAccessForReminders);
                var settings_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                var settings_sooud = await db.Settings.FindAsync(SettingsValue.GUID_ShowOnlyOwnUserData);
                var settings_resetpass = await db.Settings.FindAsync(SettingsValue.GUID_ResetPassword);
                var settings_fsh = await db.Settings.FindAsync(SettingsValue.GUID_FixSessionHours);
                var settings_prr = await db.Settings.FindAsync(SettingsValue.GUID_PayrollRatesRoles);

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

                List<string> role_for_own_data = new List<string>();
                if (!string.IsNullOrEmpty(settings_sooud.Value_String))
                {
                    string[] ids = settings_sooud.Value_String.Split(',');
                    foreach (var id in ids)
                    {
                        role_for_own_data.Add(id);
                    }
                }

                List<string> role_for_payroll_rates = new List<string>();
                if (!string.IsNullOrEmpty(settings_prr.Value_String))
                {
                    string[] ids = settings_prr.Value_String.Split(',');
                    foreach (var id in ids)
                    {
                        role_for_payroll_rates.Add(id);
                    }
                }

                SettingsViewModels settingsViewModels = new SettingsViewModels()
                {
                    AutoEntryForCashPayments = settings_aefcp.Value_Guid ?? Guid.Empty,
                    AutoEntryForCashPayments_Notes = settings_aefcp.Notes,
                    UserSetRoleAllowed = settings_usra.Value_Guid ?? Guid.Empty,
                    UserSetRoleAllowed_Notes = settings_usra.Notes,
                    RoleAccessForReminders = role_for_reminder,
                    RoleAccessForReminders_Notes = settings_rafr.Notes,
                    FullAccessForTutorSchedules = role_for_tutor_schedule,
                    FullAccessForTutorSchedules_Notes = settings_fafts.Notes,
                    ShowOnlyOwnUserData = role_for_own_data,
                    ShowOnlyOwnUserData_Notes = settings_sooud.Notes,
                    ResetPassword = settings_resetpass.Value_String,
                    ResetPassword_Notes = settings_resetpass.Notes,
                    FixSessionHours = settings_fsh.Value_String,
                    FixSessionHours_Notes = settings_fsh.Notes,
                    PayrollRatesRoles = role_for_payroll_rates,
                    PayrollRatesRoles_Notes = settings_prr.Notes
                };

                ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
                ViewBag.password = settings_resetpass.Value_String;
                return View(settingsViewModels);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index([Bind(Include = "AutoEntryForCashPayments,AutoEntryForCashPayments_Notes,UserSetRoleAllowed,UserSetRoleAllowed_Notes,RoleAccessForReminders,RoleAccessForReminders_Notes,FullAccessForTutorSchedules,FullAccessForTutorSchedules_Notes,ShowOnlyOwnUserData,ShowOnlyOwnUserData_Notes,ResetPassword,ResetPassword_Notes,PayrollRatesRoles,PayrollRatesRoles_Notes")] SettingsViewModels settingsViewModels)
        {
            if (settingsViewModels.AutoEntryForCashPayments == Guid.Empty || settingsViewModels.AutoEntryForCashPayments == null)
            {
                //ModelState.AddModelError("AutoEntryForCashPayments", "The field Auto Entry for Cash Payments is required.");
                ModelState.AddModelError("ValidationMessage", "The field Auto Entry for Cash Payments is required.");
            }

            if (settingsViewModels.RoleAccessForReminders == null)
            {
                //ModelState.AddModelError("RoleAccessForReminders", "The field Role for Reminders is required.");
                ModelState.AddModelError("ValidationMessage", "The field Role for Reminders is required.");
            }

            if (settingsViewModels.FullAccessForTutorSchedules == null)
            {
                //ModelState.AddModelError("FullAccessForTutorSchedules", "The field Role for Tutor Schedule is required.");
                ModelState.AddModelError("ValidationMessage", "The field Role for Tutor Schedule is required.");
            }

            if (settingsViewModels.ShowOnlyOwnUserData == null)
            {
                ModelState.AddModelError("ValidationMessage", "The field Show Only Own User Data is required.");
            }

            if (settingsViewModels.UserSetRoleAllowed == Guid.Empty || settingsViewModels.UserSetRoleAllowed == null)
            {
                //ModelState.AddModelError("UserSetRoleAllowed", "The field User Set Role Allowed is required.");
                ModelState.AddModelError("ValidationMessage", "The field User Set Role Allowed is required.");
            }

            if (string.IsNullOrEmpty(settingsViewModels.ResetPassword))
            {
                ModelState.AddModelError("ValidationMessage", "The field Reset Password is required.");
            }
            else
            {
                if (settingsViewModels.ResetPassword.Length < 6)
                {
                    ModelState.AddModelError("ValidationMessage", "The field Reset Password must be 6 characters or more.");
                }
            }

            if (settingsViewModels.PayrollRatesRoles == null)
            {
                ModelState.AddModelError("ValidationMessage", "The field Payroll Rates Roles is required.");
            }

            if (ModelState.IsValid)
            {
                SettingsModels settingsModels_aefcp = await db.Settings.FindAsync(SettingsValue.GUID_AutoEntryForCashPayments);
                settingsModels_aefcp.Value_Guid = settingsViewModels.AutoEntryForCashPayments;
                settingsModels_aefcp.Notes = settingsViewModels.AutoEntryForCashPayments_Notes;
                db.Entry(settingsModels_aefcp).State = EntityState.Modified;

                SettingsModels settingsModels_usra = await db.Settings.FindAsync(SettingsValue.GUID_UserSetRoleAllowed);
                settingsModels_usra.Value_Guid = settingsViewModels.UserSetRoleAllowed;
                settingsModels_usra.Notes = settingsViewModels.UserSetRoleAllowed_Notes;
                db.Entry(settingsModels_usra).State = EntityState.Modified;

                SettingsModels settingsModels_rafr = await db.Settings.FindAsync(SettingsValue.GUID_RoleAccessForReminders);
                settingsModels_rafr.Value_String = string.Join(",", settingsViewModels.RoleAccessForReminders);
                settingsModels_rafr.Notes = settingsViewModels.RoleAccessForReminders_Notes;
                db.Entry(settingsModels_rafr).State = EntityState.Modified;

                SettingsModels settingsModels_fafts = await db.Settings.FindAsync(SettingsValue.GUID_FullAccessForTutorSchedule);
                settingsModels_fafts.Value_String = string.Join(",", settingsViewModels.FullAccessForTutorSchedules);
                settingsModels_fafts.Notes = settingsViewModels.FullAccessForTutorSchedules_Notes;
                db.Entry(settingsModels_fafts).State = EntityState.Modified;

                SettingsModels settingsModels_sooud = await db.Settings.FindAsync(SettingsValue.GUID_ShowOnlyOwnUserData);
                settingsModels_sooud.Value_String = string.Join(",", settingsViewModels.ShowOnlyOwnUserData);
                settingsModels_sooud.Notes = settingsViewModels.ShowOnlyOwnUserData_Notes;
                db.Entry(settingsModels_sooud).State = EntityState.Modified;

                SettingsModels settingsModels_resetpass = await db.Settings.FindAsync(SettingsValue.GUID_ResetPassword);
                settingsModels_resetpass.Value_String = settingsViewModels.ResetPassword;
                settingsModels_resetpass.Notes = settingsViewModels.ResetPassword_Notes;
                db.Entry(settingsModels_resetpass).State = EntityState.Modified;

                SettingsModels settingsModels_prr = await db.Settings.FindAsync(SettingsValue.GUID_PayrollRatesRoles);
                settingsModels_prr.Value_String = string.Join(",", settingsViewModels.PayrollRatesRoles);
                settingsModels_prr.Notes = settingsViewModels.PayrollRatesRoles_Notes;
                db.Entry(settingsModels_prr).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.password = settingsViewModels.ResetPassword;
            return View(settingsViewModels);
        }
    }
}