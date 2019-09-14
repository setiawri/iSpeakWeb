using iSpeak.Common;
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
    public class PettyCashRecordsController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        #region GetPettyCash
        public async Task<JsonResult> GetPettyCash(Guid branch_id, DateTime start, DateTime end, string category_id, bool is_not_approve)
        {
            DateTime fromDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(start.Year, start.Month, start.Day, 0, 0, 0));
            DateTime toDate = TimeZoneInfo.ConvertTimeToUtc(new DateTime(end.Year, end.Month, end.Day, 23, 59, 59));
            var items = (is_not_approve)
                //show not approved
                ? await (from pcr in db.PettyCashRecords
                         join pcc in db.PettyCashRecordsCategories on pcr.PettyCashRecordsCategories_Id equals pcc.Id
                         where pcr.Branches_Id == branch_id && pcr.Timestamp > fromDate && pcr.Timestamp < toDate && pcr.PettyCashRecordsCategories_Id.ToString().Contains(category_id) && pcr.IsChecked == false
                         orderby pcr.No
                         select new { pcr, pcc }).ToListAsync()
                //show all
                : await (from pcr in db.PettyCashRecords
                         join pcc in db.PettyCashRecordsCategories on pcr.PettyCashRecordsCategories_Id equals pcc.Id
                         where pcr.Branches_Id == branch_id && pcr.Timestamp > fromDate && pcr.Timestamp < toDate && pcr.PettyCashRecordsCategories_Id.ToString().Contains(category_id)
                         orderby pcr.No
                         select new { pcr, pcc }).ToListAsync();
            List<PettyCashViewModels> list = new List<PettyCashViewModels>();
            int balance = 0;
            foreach (var item in items)
            {
                var data_user = db.User.Where(x => x.Id == item.pcr.UserAccounts_Id).FirstOrDefault();
                string user_input = data_user == null ? "" : data_user.Firstname + " " + data_user.Middlename + " " + data_user.Lastname;
                list.Add(new PettyCashViewModels
                {
                    Id = item.pcr.Id,
                    No = item.pcr.No,
                    Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.pcr.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("yyyy/MM/dd HH:mm"),
                    Category = item.pcc.Name,
                    Notes = item.pcr.Notes,
                    Amount = item.pcr.Amount,
                    Balance = balance += item.pcr.Amount,
                    IsChecked = item.pcr.IsChecked,
                    Status_render = (item.pcr.IsChecked) ? "<a href='#'><span onclick='CancelApprove(\"" + item.pcr.Id + "\",\"" + item.pcr.Notes + "\",\"" + item.pcr.Amount + "\")' class='badge badge-success'>Approved</span></a>" : "<a href='#'><span onclick='Approve(\"" + item.pcr.Id + "\",\"" + item.pcr.Notes + "\",\"" + item.pcr.Amount + "\")' class='badge badge-dark'>None</span></a>",
                    UserInput = user_input
                    //Action_render = "<a href='" + Url.Content("~") + "PettyCashRecords/Edit/" + item.pcr.Id + "'>Edit</a> | <a href='" + Url.Content("~") + "PettyCashRecords/Delete/" + item.pcr.Id + "'>Delete</a>"
                });
            }

            return Json(new { list, balance }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region SetApproved
        public async Task<JsonResult> SetApproved(Guid pettycash_id)
        {
            string status = "OK";
            PettyCashRecordsModels pettyCashRecordsModels = await db.PettyCashRecords.Where(x => x.Id == pettycash_id).FirstOrDefaultAsync();
            pettyCashRecordsModels.IsChecked = true;
            db.Entry(pettyCashRecordsModels).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Json(new { status }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region CancelApproved
        public async Task<JsonResult> CancelApproved(Guid pettycash_id)
        {
            string status = "OK";
            PettyCashRecordsModels pettyCashRecordsModels = await db.PettyCashRecords.Where(x => x.Id == pettycash_id).FirstOrDefaultAsync();
            pettyCashRecordsModels.IsChecked = false;
            db.Entry(pettyCashRecordsModels).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Json(new { status }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Index()
        {
            Permission p = new Permission();
            bool auth = p.IsGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                ViewBag.Approve = p.IsGranted(User.Identity.Name, "pettycashrecords_approve");
                ViewBag.listCategory = new SelectList(db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name), "Id", "Name");
                ViewBag.initDateStart = DateTime.UtcNow.AddMonths(-1);
                return View();
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
                ViewBag.categories = db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList();
                var selected = db.PettyCashRecordsCategories.Where(x => x.Active == true && x.Default_row == true).FirstOrDefault();
                ViewBag.categorySelectedId = (selected != null) ? selected.Id.ToString() : "";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Branches_Id,No,Timestamp,PettyCashRecordsCategories_Id,Amount,Notes")] PettyCashRecordsModels pettyCashRecordsModels)
        {
            if (ModelState.IsValid)
            {
                string lastHex_string = db.PettyCashRecords.AsNoTracking().Max(x => x.No);
                int lastHex_int = int.Parse(
                    string.IsNullOrEmpty(lastHex_string) ? 0.ToString("X5") : lastHex_string,
                    System.Globalization.NumberStyles.HexNumber);

                pettyCashRecordsModels.Id = Guid.NewGuid();
                pettyCashRecordsModels.No = (lastHex_int + 1).ToString("X5");
                pettyCashRecordsModels.Timestamp = DateTime.UtcNow;
                pettyCashRecordsModels.IsChecked = false;
                pettyCashRecordsModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
                db.PettyCashRecords.Add(pettyCashRecordsModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.categories = db.PettyCashRecordsCategories.Where(x => x.Active == true).OrderBy(x => x.Name).ToList();
            var selected = db.PettyCashRecordsCategories.Where(x => x.Active == true && x.Default_row == true).FirstOrDefault();
            ViewBag.categorySelectedId = (selected != null) ? selected.Id.ToString() : "";
            return View(pettyCashRecordsModels);
        }
    }
}