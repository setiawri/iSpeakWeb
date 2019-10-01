using iSpeak.Common;
using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        #region Get Olders
        public JsonResult GetOlders(string description)
        {
            Permission p = new Permission();
            bool cancel_access = p.IsGranted(User.Identity.Name, "files_deletefileupload");
            var list = db.UploadFiles.Where(x => x.Description == description && x.IsOlder == true).OrderBy(x => x.Timestamp).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Description</th>
                                                <th>Filename</th>
                                                <th>Action</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                string url_delete = cancel_access ? "<a href='" + Url.Content("~") + "Files/DeleteFileUpload/" + item.Id + "'>Delete</a>" : string.Empty;
                message += @"<tr>
                                <td>" + string.Format("{0:yyyy/MM/dd HH:mm}", TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))) + @"</td>
                                <td>" + item.Description + @"</td>
                                <td><a href='" + Url.Content("~") + "Files/Download/" + item.Id + "'>" + item.Filename + @"</a></td>
                                <td>" + url_delete + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public async Task<ActionResult> Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                Guid user_branch = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Branches_Id;
                var result = await db.UploadFiles.Where(x => x.IsOlder == false && (x.Branches_Id == user_branch || x.Branches_Id == null)).ToListAsync();
                List<UploadFilesModels> list = new List<UploadFilesModels>();
                foreach (var item in result)
                {
                    list.Add(new UploadFilesModels
                    {
                        Id = item.Id,
                        Branches_Id = item.Branches_Id,
                        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                        Filename = item.Filename,
                        Description = item.Description,
                        IsOlder = item.IsOlder
                    });
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
                ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,Timestamp,Filename,Description,IsOlder")] UploadFilesModels uploadFilesModels, List<HttpPostedFileBase> files)
        {
            if (files[0] == null)
            {
                ModelState.AddModelError("Document", "File(s) is required.");
            }

            if (ModelState.IsValid)
            {
                string Dir = Server.MapPath("~/assets/files/");
                if (!Directory.Exists(Dir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(Dir);
                }
                
                foreach (HttpPostedFileBase file in files)
                {
                    uploadFilesModels.Id = Guid.NewGuid();
                    uploadFilesModels.Timestamp = DateTime.UtcNow;
                    uploadFilesModels.Filename = file.FileName;
                    uploadFilesModels.IsOlder = false;
                    db.UploadFiles.Add(uploadFilesModels);

                    file.SaveAs(Path.Combine(Dir, uploadFilesModels.Id.ToString() + "_" + uploadFilesModels.Filename + Path.GetExtension(file.FileName)));
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(uploadFilesModels);
        }

        public async Task<ActionResult> Update(Guid id)
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                UploadFilesModels uploadFilesModels = await db.UploadFiles.FindAsync(id);
                return View(uploadFilesModels);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update([Bind(Include = "Id,Branches_Id,Timestamp,Filename,Description,IsOlder")] UploadFilesModels uploadFilesModels, List<HttpPostedFileBase> files)
        {
            if (files[0] == null)
            {
                ModelState.AddModelError("Document", "File(s) is required.");
            }

            if (ModelState.IsValid)
            {
                string Dir = Server.MapPath("~/assets/files/");
                if (!Directory.Exists(Dir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(Dir);
                }

                UploadFilesModels model_before = await db.UploadFiles.FindAsync(uploadFilesModels.Id);
                model_before.IsOlder = true;
                db.Entry(model_before).State = EntityState.Modified;

                foreach (HttpPostedFileBase file in files)
                {
                    uploadFilesModels.Id = Guid.NewGuid();
                    uploadFilesModels.Timestamp = DateTime.UtcNow;
                    uploadFilesModels.Filename = file.FileName;
                    uploadFilesModels.IsOlder = false;
                    db.UploadFiles.Add(uploadFilesModels);

                    file.SaveAs(Path.Combine(Dir, uploadFilesModels.Id.ToString() + "_" + uploadFilesModels.Filename + Path.GetExtension(file.FileName)));
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(uploadFilesModels);
        }

        public async Task<ActionResult> Download(Guid id)
        {
            UploadFilesModels uploadFilesModels = await db.UploadFiles.FindAsync(id);
            string path = Server.MapPath("~/assets/files/" + id.ToString() + "_" + uploadFilesModels.Filename + Path.GetExtension(uploadFilesModels.Filename));
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(path, contentType, Path.GetFileName(path));
        }

        public async Task<ActionResult> DeleteFileUpload(Guid id)
        {
            UploadFilesModels uploadFilesModels = await db.UploadFiles.FindAsync(id);
            db.UploadFiles.Remove(uploadFilesModels);

            string Dir = Server.MapPath("~/assets/files/");
            string fileName = uploadFilesModels.Id.ToString() + "_" + uploadFilesModels.Filename + Path.GetExtension(uploadFilesModels.Filename);
            if (System.IO.File.Exists(Dir + fileName))
                System.IO.File.Delete(Dir + fileName);

            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}