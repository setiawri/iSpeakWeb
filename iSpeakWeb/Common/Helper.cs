using System;
using System.Web;
using System.Web.Mvc;
using LIBUtil;

namespace iSpeak
{
    public class Helper
    {
        public static DateTime setFilterViewBag(ControllerBase controller, int? year, int? month, string search, string periodChange)
        {
            DateTime payPeriod;

            if (month == null || year == null)
                payPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
            else
                payPeriod = new DateTime((int)year, (int)month, 1, 0, 0, 0);

            if (periodChange == EnumActions.Previous.ToString())
                payPeriod = payPeriod.AddMonths(-1);
            else if (periodChange == EnumActions.Next.ToString())
                payPeriod = payPeriod.AddMonths(1);

            var ViewBag = controller.ViewBag;
            ViewBag.PayPeriodYear = Util.validateParameter(payPeriod.Year);
            ViewBag.PayPeriodMonth = Util.validateParameter(payPeriod.Month);
            ViewBag.PayPeriod = Util.validateParameter(payPeriod);
            ViewBag.Search = Util.validateParameter(search);

            return payPeriod;
        }

    }
}