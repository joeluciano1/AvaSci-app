#define Graph_And_Chart_PRO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartAndGraph
{
    /// <summary>
    /// holds date values
    /// </summary>
    public class ChartDateUtility
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);

        public static double TimeSpanToValue(TimeSpan span)
        {
            return span.TotalSeconds;
        }
        public static double DateToValue(DateTime dateTime)
        {
            return (dateTime - Epoch).TotalSeconds;
        }

        public static string DateToTimeString(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        public static string DateToDateString(DateTime dateTime)
        {
            return dateTime.ToString("d");
        }
        public static string DateToDateTimeString(DateTime dateTime,Func<DateTime,string> customFormat)
        {
            if(customFormat != null)
            {
                try
                {
                    return customFormat(dateTime);
                }
                catch
                {

                }
            }
            return string.Format("{0}{1}{2}", dateTime.ToString("d"), Environment.NewLine, dateTime.ToString("t"));
        }
        public static DateTime ValueToDate(double value)
        {
            return Epoch.AddSeconds(value);
        }
    }
}
