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
    public class SaleController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            var data = (from si in db.SaleInvoices
                        join b in db.Branches on si.Branches_Id equals b.Id
                        join u in db.User on si.Customer_UserAccounts_Id equals u.Id
                        select new SaleInvoicesIndexModels
                        {
                            Id = si.Id,
                            Branches = b.Name,
                            No = si.No,
                            Timestamp = si.Timestamp,
                            Customer = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Amount = si.Amount,
                            Cancelled = si.Cancelled
                        }).ToListAsync();
            return View(await data);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.listBranch = new SelectList(db.Branches.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listLesson = new SelectList(db.LessonPackages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listVoucher = new SelectList(db.Vouchers.Where(x => x.Active == true).OrderBy(x => x.Code).ToList(), "Id", "Code");
            var customers = (from u in db.User
                            join ur in db.UserRole on u.Id equals ur.UserId
                            join r in db.Role on ur.RoleId equals r.Id
                            where r.Name == "Student"
                            orderby u.Firstname
                            select new { u }).ToList();
            List<object> customer_list = new List<object>();
            foreach (var item in customers)
            {
                customer_list.Add(new
                {
                    Id = item.u.Id,
                    Name = item.u.Firstname + " " + item.u.Middlename + " " + item.u.Lastname
                });
            }
            ViewBag.listCustomer = new SelectList(customer_list, "Id", "Name");

            return View();
        }

        public JsonResult GetItemTotal(int qty, Guid lesson_package_id, decimal disc, string voucher_id)
        {
            int price = db.LessonPackages.Where(x => x.Id == lesson_package_id).FirstOrDefault().Price;
            decimal voucher = string.IsNullOrEmpty(voucher_id) ? 0 : db.Vouchers.Where(x => x.Id.ToString() == voucher_id).FirstOrDefault().Amount;
            decimal subtotal = (qty * price) - disc - voucher;

            return Json(new { price = price, voucher = voucher, subtotal = subtotal }, JsonRequestBehavior.AllowGet);
        }
    }
}