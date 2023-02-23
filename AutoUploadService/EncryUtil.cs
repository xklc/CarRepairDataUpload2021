using System;
using System.Management;
using System.Runtime.InteropServices;

namespace AutoUploadService
{


    public class EncryUtil
    {
        [DllImport(@"md5.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "md5")]
        public static extern IntPtr md5(String key);

        public static String getEcryptedHardDiskId()
        {
            //System.Man
            ////获取硬盘ID
            String HDid = "";
            ManagementClass cimobject1 = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo in moc1)
            {
                HDid = (string)mo.Properties["Model"].Value;
                break;
            }
            string hard_disk_id = Marshal.PtrToStringAnsi(md5(HDid));
            ////  Exception error = Server.GetLastError();
            //int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            //Win32Exception exc = new Win32Exception(errorCode);
            //MessageBox.Show(exc.Message);

            return hard_disk_id;
        }
    }
}
