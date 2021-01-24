using System;
using System.Collections.Generic;
using System.Text;

namespace HandyUploadForm
{
    public class StringUtil
    {
        public static Boolean  isEmpty(String str)
        {
            return (str == null || str.Trim().Length == 0);
        }
    }
}
