using System;
using System.Globalization;

namespace TeamTaskManager.Extensions
{
    public static class DateExtensions
    {
        public static string ToShamsi(this DateTime date, bool includeTime = false)
        {
            if (date == DateTime.MinValue)
                return "نامشخص";

            var pc = new PersianCalendar();
            var result = $"{pc.GetYear(date)}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";

            if (includeTime)
                result += $" - {date:HH:mm}";

            return result;
        }

    }
}
