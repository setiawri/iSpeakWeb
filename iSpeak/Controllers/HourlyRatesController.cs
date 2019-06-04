using iSpeak.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iSpeak.Controllers
{
    [Authorize]
    public class HourlyRatesController : Controller
    {
        private iSpeakContext db = new iSpeakContext();

        public async Task<ActionResult> Index()
        {
            var data = (from hr in db.HourlyRates
                        join lp in db.LessonPackages on hr.LessonPackages_Id equals lp.Id
                        join u in db.User on hr.UserAccounts_Id equals u.Id
                        orderby lp.Name
                        select new HourlyRatesViewModels
                        {
                            Id = hr.Id,
                            LessonPackages = lp.Name,
                            UserAccounts = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Rate = hr.Rate,
                            Notes = hr.Notes
                        }).ToListAsync();
            return View(await data);
        }

        public ActionResult Create()
        {
            ViewBag.listLessonPackage = new SelectList(db.LessonPackages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<object> newList = new List<object>();
            foreach (var item in db.User.Where(x => x.Active == true).OrderBy(x => x.Firstname).ToList())
            {
                newList.Add(new
                {
                    Id = item.Id,
                    Name = item.Firstname + " " + item.Middlename + " " + item.Lastname
                });
            }
            ViewBag.listUser = new SelectList(newList, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
        {
            if (ModelState.IsValid)
            {
                hourlyRatesModels.Id = Guid.NewGuid();
                db.HourlyRates.Add(hourlyRatesModels);

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listLessonPackage = new SelectList(db.LessonPackages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<object> newList = new List<object>();
            foreach (var item in db.User.Where(x => x.Active == true).OrderBy(x => x.Firstname).ToList())
            {
                newList.Add(new
                {
                    Id = item.Id,
                    Name = item.Firstname + " " + item.Middlename + " " + item.Lastname
                });
            }
            ViewBag.listUser = new SelectList(newList, "Id", "Name");
            return View(hourlyRatesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HourlyRatesModels hourlyRatesModels = await db.HourlyRates.FindAsync(id);
            if (hourlyRatesModels == null)
            {
                return HttpNotFound();
            }

            ViewBag.listLessonPackage = new SelectList(db.LessonPackages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<object> newList = new List<object>();
            foreach (var item in db.User.Where(x => x.Active == true).OrderBy(x => x.Firstname).ToList())
            {
                newList.Add(new
                {
                    Id = item.Id,
                    Name = item.Firstname + " " + item.Middlename + " " + item.Lastname
                });
            }
            ViewBag.listUser = new SelectList(newList, "Id", "Name");
            return View(hourlyRatesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,LessonPackages_Id,UserAccounts_Id,Rate,Notes")] HourlyRatesModels hourlyRatesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hourlyRatesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listLessonPackage = new SelectList(db.LessonPackages.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<object> newList = new List<object>();
            foreach (var item in db.User.Where(x => x.Active == true).OrderBy(x => x.Firstname).ToList())
            {
                newList.Add(new
                {
                    Id = item.Id,
                    Name = item.Firstname + " " + item.Middlename + " " + item.Lastname
                });
            }
            ViewBag.listUser = new SelectList(newList, "Id", "Name");
            return View(hourlyRatesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from hr in db.HourlyRates
                        join lp in db.LessonPackages on hr.LessonPackages_Id equals lp.Id
                        join u in db.User on hr.UserAccounts_Id equals u.Id
                        where hr.Id == id
                        select new HourlyRatesViewModels
                        {
                            Id = hr.Id,
                            LessonPackages = lp.Name,
                            UserAccounts = u.Firstname + " " + u.Middlename + " " + u.Lastname,
                            Rate = hr.Rate,
                            Notes = hr.Notes
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            HourlyRatesModels hourlyRatesModels = await db.HourlyRates.FindAsync(id);
            db.HourlyRates.Remove(hourlyRatesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}