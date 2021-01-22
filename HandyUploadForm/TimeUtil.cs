using System;
using System.Collections.Generic;
using System.Text;

namespace HandyUploadForm
{
    public class TimeUtil
    {
        public static long getCurrentSeconds()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (currentTicks - dtFrom.Ticks) / 10000000;
        }
    }
}
