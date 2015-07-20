using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeExtensions
    {
        public static long AsJSTime(this DateTime date)
        {
            var timeBegin = new DateTime(1970, 1, 1);
            return (date.Ticks / 10000L - timeBegin.Ticks / 10000L);
        }

        public static DateTime FromJSTime(this DateTime date, long secs)
        {
            var ticks = secs * 10000L;
            var timeBegin = new DateTime(1970, 1, 1);
            date = new DateTime(ticks + timeBegin.Ticks);
            return new DateTime(ticks + timeBegin.Ticks);
        }
    }
}
