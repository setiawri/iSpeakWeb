using iSpeak.Models;
using iSpeak.Models.API;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace iSpeak.Controllers.API
{
    public class SchedulesController : ApiController
    {
        private readonly iSpeakContext db = new iSpeakContext();

        [AllowAnonymous]
        [HttpPost]
        [Route("api/schedules")]
        public async Task<HttpResponseMessage> Schedules(CommonRequestModels model)
        {
            var user_login = await (from u in db.User
                                    join ur in db.UserRole on u.Id equals ur.UserId
                                    join r in db.Role on ur.RoleId equals r.Id
                                    where u.UserName == model.Username
                                    select new { u, ur, r }).FirstOrDefaultAsync();
            List<ScheduleApiModels> list = new List<ScheduleApiModels>();
            if (user_login.r.Name.ToLower() == "student")
            {
                var items = await (from tss in db.TutorStudentSchedules
                                   join s in db.User on tss.Student_UserAccounts_Id equals s.Id
                                   join t in db.User on tss.Tutor_UserAccounts_Id equals t.Id
                                   join sii in db.SaleInvoiceItems on tss.InvoiceItems_Id equals sii.Id
                                   join lp in db.LessonPackages on sii.LessonPackages_Id equals lp.Id
                                   where tss.Student_UserAccounts_Id == user_login.u.Id
                                   orderby tss.DayOfWeek
                                   select new { tss, s, t, sii, lp }).ToListAsync();

                foreach (var item in items)
                {
                    list.Add(new ScheduleApiModels
                    {
                        Schedule_Id = item.tss.Id,
                        TimeSchedule = string.Format("{0}, {1:HH:mm} - {2:HH:mm}", item.tss.DayOfWeek.ToString(), item.tss.StartTime, item.tss.EndTime),
                        Lesson = item.lp.Name,
                        Tutor = string.Format("Tutor: {0} {1} {2}", item.t.Firstname, item.t.Middlename, item.t.Lastname),
                        Notes = string.IsNullOrEmpty(item.tss.Notes) ? string.Empty : string.Format("Notes: {0}", item.tss.Notes),
                        Role = "student"
                    });
                }
            }
            else //tutor
            {
                var items = await (from ts in db.TutorSchedules
                                   join u in db.User on ts.Tutor_UserAccounts_Id equals u.Id
                                   where ts.Tutor_UserAccounts_Id == user_login.u.Id
                                   orderby ts.DayOfWeek
                                   select new { ts, u }).ToListAsync();

                foreach (var item in items)
                {
                    list.Add(new ScheduleApiModels
                    {
                        Schedule_Id = item.ts.Id,
                        TimeSchedule = string.Format("Tutor: {0} {1} {2}", item.u.Firstname, item.u.Middlename, item.u.Lastname),
                        Lesson = string.Format("{0}, {1:HH:mm} - {2:HH:mm}", item.ts.DayOfWeek.ToString(), item.ts.StartTime, item.ts.EndTime),
                        Tutor = string.IsNullOrEmpty(item.ts.Notes) ? string.Empty : string.Format("Notes: {0}", item.ts.Notes),
                        Notes = string.Empty,
                        Role = "tutor"
                    });
                }
            }

            if (list == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/tutorschedules")]
        public async Task<HttpResponseMessage> TutorSchedules(CommonRequestModels model)
        {
            var result = await db.TutorSchedules.Where(x => x.Id.ToString() == model.ReffId).FirstOrDefaultAsync();

            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, result);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/scheduleadd")]
        public async Task<HttpResponseMessage> ScheduleAdd(TutorScheduleApiModels model)
        {
            string user_id = db.User.Where(x => x.UserName == model.Username).FirstOrDefault().Id;
            DayOfWeekEnum dow = (DayOfWeekEnum)Enum.Parse(typeof(DayOfWeekEnum), model.Day);
            DateTime _start = DateTime.Parse(model.Start);
            DateTime start = new DateTime(1970, 1, 1, _start.Hour, _start.Minute, 0);
            DateTime _end = DateTime.Parse(model.End);
            DateTime end = new DateTime(1970, 1, 1, _end.Hour, _end.Minute, 0);

            CommonRequestModels data = new CommonRequestModels();

            var isExist = await db.TutorSchedules
                .Where(x => x.Tutor_UserAccounts_Id == user_id
                    && x.DayOfWeek == dow
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();

            if (isExist != null)
            {
                data.Message = "This schedule already exist.";
            }
            else
            {
                if (start.Hour > end.Hour || (start.Hour == end.Hour && start.Minute > end.Minute))
                {
                    data.Message = "The Start Time cannot greater than End Time";
                }
            }

            if (string.IsNullOrEmpty(data.Message))
            {
                TutorSchedulesModels tutorSchedulesModels = new TutorSchedulesModels
                {
                    Id = Guid.NewGuid(),
                    Tutor_UserAccounts_Id = user_id,
                    DayOfWeek = dow,
                    StartTime = start,
                    EndTime = end,
                    IsActive = true,
                    Notes = model.Notes
                };
                db.TutorSchedules.Add(tutorSchedulesModels);
                await db.SaveChangesAsync();
            }

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/scheduleedit")]
        public async Task<HttpResponseMessage> ScheduleEdit(TutorScheduleApiModels model)
        {
            string user_id = db.User.Where(x => x.UserName == model.Username).FirstOrDefault().Id;
            DayOfWeekEnum dow = (DayOfWeekEnum)Enum.Parse(typeof(DayOfWeekEnum), model.Day);
            DateTime _start = DateTime.Parse(model.Start);
            DateTime start = new DateTime(1970, 1, 1, _start.Hour, _start.Minute, 0);
            DateTime _end = DateTime.Parse(model.End);
            DateTime end = new DateTime(1970, 1, 1, _end.Hour, _end.Minute, 0);

            CommonRequestModels data = new CommonRequestModels();

            var isExist = await db.TutorSchedules
                .Where(x => x.Id.ToString() != model.Schedule_Id
                    && x.Tutor_UserAccounts_Id == user_id
                    && x.DayOfWeek == dow
                    && x.StartTime == start && x.EndTime == end).FirstOrDefaultAsync();

            if (isExist != null)
            {
                data.Message = "This schedule already exist.";
            }
            else
            {
                if (start.Hour > end.Hour || (start.Hour == end.Hour && start.Minute > end.Minute))
                {
                    data.Message = "The Start Time cannot greater than End Time";
                }
            }

            if (string.IsNullOrEmpty(data.Message))
            {
                var current_data = await db.TutorSchedules.Where(x => x.Id.ToString() == model.Schedule_Id).FirstOrDefaultAsync();
                current_data.Tutor_UserAccounts_Id = user_id;
                current_data.DayOfWeek = dow;
                current_data.StartTime = start;
                current_data.EndTime = end;
                //current_data.IsActive = tutorSchedulesModels.IsActive;
                current_data.Notes = model.Notes;
                db.Entry(current_data).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }
    }
}
