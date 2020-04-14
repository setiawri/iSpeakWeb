using iSpeak.Common;
using iSpeak.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    public class RoleController : Controller
    {
        private iSpeakContext db = new iSpeakContext();
        private ApplicationRoleManager _roleManager;

        public RoleController()
        {
        }

        public RoleController(ApplicationRoleManager roleManager)
        {
            RoleManager = roleManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // GET: Role
        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Log = p.IsGranted(User.Identity.Name, "logs_view");
                List<RoleViewModels> list = new List<RoleViewModels>();
                foreach (var role in RoleManager.Roles.OrderBy(x => x.Name))
                {
                    list.Add(new RoleViewModels(role));
                }
                return View(list);
            }
        }

        public ActionResult Create()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModels model)
        {
            var role = new ApplicationRole() { Name = model.Name };
            await RoleManager.CreateAsync(role);
            LogsModels logs_add = new LogsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                TableName = "AspNetRoles",
                RefId = new Guid(role.Id),
                Action = "Added",
                ColumnName = "*ALL",
                UserAccounts_Id = User.Identity.GetUserId()
            };
            db.Logs.Add(logs_add);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var role = await RoleManager.FindByIdAsync(id);
                return View(new RoleViewModels(role));
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleViewModels model)
        {
            var current_data = await db.Role.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            var role = new ApplicationRole() { Id = model.Id, Name = model.Name };
            await RoleManager.UpdateAsync(role);
            
            current_data.Name = model.Name;
            db.Entry(current_data).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //public async Task<ActionResult> Delete(string id)
        //{
        //    Permission p = new Permission();
        //    bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
        //    if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
        //    else
        //    {
        //        var role = await RoleManager.FindByIdAsync(id);
        //        return View(new RoleViewModels(role));
        //    }
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(string id)
        //{
        //    var role = await RoleManager.FindByIdAsync(id);
        //    await RoleManager.DeleteAsync(role);
        //    return RedirectToAction("Index");
        //}

        public ActionResult Manage(string id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Id = id;
                ViewBag.RoleName = db.Role.Where(x => x.Id == id).FirstOrDefault().Name;

                return View();
            }
        }

        public JsonResult GetAccessMenu(string id)
        {
            var menus = (from m in db.MasterMenu
                         join a in db.Access
                             .Join(db.Role, x => x.RoleId, y => y.Id, (x, y) => new { x.WebMenuAccess, RoleName = y.Name, RoleId = y.Id })
                             .Where(x => x.RoleId == id)
                         on m.WebMenuAccess equals a.WebMenuAccess into joined
                         from access in joined.DefaultIfEmpty()
                         where m.ParentOrder > 0 // nav menu not loaded
                         orderby m.ParentOrder, m.MenuOrder
                         select new AccessViewModels
                         {
                             MenuOrder = m.MenuOrder,
                             ParentMenu = m.ParentMenu,
                             MenuName = m.MenuName,
                             Notes = m.Notes,
                             WebMenuAccess = m.WebMenuAccess.ToUpper(),
                             RoleName = access.RoleName ?? string.Empty,
                             IsSelected = string.IsNullOrEmpty(access.RoleName) ? false : true
                         }).ToList();

            string content = ""; int row = 1;
            foreach (var menu in menus)
            {
                //string action = string.IsNullOrEmpty(menu.RoleName)
                //    ? "<a href='#'><span class='badge badge-dark d-block' onclick='ActionMenu(\"" + menu.WebMenuAccess + "\",\"" + id + "\",\"enable\")'>Disabled</span></a>"
                //    : "<a href='#'><span class='badge badge-success d-block' onclick='ActionMenu(\"" + menu.WebMenuAccess + "\",\"" + id + "\",\"disabled\")'>Enabled</span></a>";

                string status_access = string.IsNullOrEmpty(menu.RoleName)
                    ? "<span class='badge badge-dark d-block'>Disabled</span>"
                    : "<span class='badge badge-success d-block'>Enabled</span>";

                string selected = menu.IsSelected ? "checked" : "";

                content += @"<tr>
                                <td>" + row + @"</td>
                                <td><input type='checkbox' id='" + menu.WebMenuAccess + "' class='check-styled' " + selected + @"/></td>
                                <td>" + menu.ParentMenu + @"</td>
                                <td>" + menu.MenuName + @"</td>
                                <td><div class='input-group'><input type='text' class='form-control input-sm' title='" + menu.Notes + "' value='" + menu.Notes + "' readonly/><span class='input-group-append'><button type='button' class='btn btn-sm btn-light' data-toggle='modal' data-target='#modal_notes' onclick='EditNotes(\"" + menu.WebMenuAccess + "\",\"" + menu.Notes + "\"" + @")'>Edit</button></span></div></td>
                                <td>" + status_access + @"</td>
                            </tr>";
                row++;
            }

            return Json(new { status = "200", content }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> UpdateNotes(string webmenu, string notes)
        {
            var mastermenu = await db.MasterMenu.Where(x => x.WebMenuAccess.ToLower() == webmenu.ToLower()).FirstOrDefaultAsync();
            mastermenu.Notes = notes;
            db.Entry(mastermenu).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveAccess(string IdRole, string Ids_selected)
        {
            List<AccessModels> list_am = db.Access.Where(x => x.RoleId == IdRole).ToList();
            if (list_am.Count>0)
            {
                foreach (var am in list_am)
                {
                    db.Access.Remove(am);
                }
            }
            db.SaveChanges();

            if (!string.IsNullOrEmpty(Ids_selected))
            {
                string[] ids = Ids_selected.Split(',');
                if (ids.Length > 0)
                {
                    foreach (string id in ids)
                    {
                        AccessModels am = new AccessModels
                        {
                            Id = Guid.NewGuid(),
                            RoleId = IdRole,
                            WebMenuAccess = id.ToLower()
                        };
                        db.Access.Add(am);
                    }
                }
                db.SaveChanges();
            }

            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SingleAccess(string WebMenuAccess, string IdRole, string Action)
        {
            if (Action.ToLower() == "enable")
            {
                AccessModels am = new AccessModels
                {
                    Id = Guid.NewGuid(),
                    RoleId = IdRole,
                    WebMenuAccess = WebMenuAccess.ToLower()
                };
                db.Access.Add(am);
            }
            else
            {
                AccessModels am = db.Access.Where(x => x.RoleId == IdRole && x.WebMenuAccess == WebMenuAccess).FirstOrDefault();
                db.Access.Remove(am);
            }
            db.SaveChanges();

            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BulkAccess(string IdRole, string ParentMenu, string Action)
        {
            var menus = (ParentMenu.ToLower() == "all") ? db.MasterMenu.ToList()
                : db.MasterMenu.Where(x => x.ParentMenu == ParentMenu).ToList();

            if (Action.ToLower() == "enable")
            {
                foreach (var menu in menus)
                {
                    var access_role = db.Access.AsNoTracking().Where(x => x.RoleId == IdRole && x.WebMenuAccess == menu.WebMenuAccess).FirstOrDefault();
                    if (access_role == null)
                    {
                        AccessModels am = new AccessModels
                        {
                            Id = Guid.NewGuid(),
                            RoleId = IdRole,
                            WebMenuAccess = menu.WebMenuAccess
                        };
                        db.Access.Add(am);
                    }
                }
            }
            else
            {
                foreach (var menu in menus)
                {
                    AccessModels am = db.Access.Where(x => x.RoleId == IdRole && x.WebMenuAccess == menu.WebMenuAccess).FirstOrDefault();
                    if (am != null)
                        db.Access.Remove(am);
                }
            }
            db.SaveChanges();

            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
    }
}