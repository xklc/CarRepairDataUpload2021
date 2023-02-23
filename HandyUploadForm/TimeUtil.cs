using System;
using System.Collections.Generic;
using System.Text;

namespace HandyUploadForm
{
    public class TimeUtil
    {
        public static long getCurrentSeconds()
        {
            TimeSpan ts = DateTime.Now - Convert.ToDateTime("1970-1-1").ToLocalTime();
            return (long)ts.TotalSeconds;
        }

        public static long getCurrentMillSeconds()
        {
            TimeSpan ts = DateTime.Now - Convert.ToDateTime("1970-1-1").ToLocalTime();
            return (long)ts.TotalMilliseconds;
        }
    }
}
