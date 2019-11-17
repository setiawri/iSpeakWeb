using iSpeak.Common;
using iSpeak.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    public class LogsController : Controller
    {
        private readonly iSpeakContext db = new iSpeakContext();

        public JsonResult Manual(string id, string table, string description)
        {
            LogsModels logs_user_add = new LogsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                TableName = table,
                RefId = new Guid(id),
                Action = "Added",
                ColumnName = "*ALL",
                Description = description,
                UserAccounts_Id = User.Identity.GetUserId()
            };
            db.Logs.Add(logs_user_add);
            db.SaveChanges();

            return Json(new { Status = "200" });
        }

        // GET: Logs
        public async Task<ActionResult> Index(string id, string ctrl, string table, string header)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, "logs_view");
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                List<LogsViewModels> vm = new List<LogsViewModels>();
                var list = await (from l in db.Logs
                                  join u in db.User on l.UserAccounts_Id equals u.Id
                                  where l.RefId.ToString() == id
                                  orderby l.Timestamp descending
                                  select new { l, u }).ToListAsync();
                foreach (var item in list)
                {
                    string description;
                    if (string.IsNullOrEmpty(item.l.Description))
                    {
                        description = item.l.Action == "Modified"
                            ? string.Format("Updated {0}: {1} to {2}", item.l.ColumnName, string.IsNullOrEmpty(item.l.OriginalValue) ? "' '" : item.l.OriginalValue, string.IsNullOrEmpty(item.l.NewValue) ? "' '" : item.l.NewValue)
                            : string.Format("{0}", item.l.Action);
                    }
                    else
                    {
                        description = item.l.Description;
                    }

                    vm.Add(new LogsViewModels
                    {
                        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.l.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                        UserInput = string.Format("{0} {1} {2}", item.u.Firstname, item.u.Middlename, item.u.Lastname),
                        Description = description
                    });
                }

                ViewBag.Id = id;
                ViewBag.ControllerName = ctrl;
                ViewBag.TableName = table;
                ViewBag.Header = header;
                return View(vm);
            }
        }
    }
}