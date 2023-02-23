using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HardDiskSer
{
    public partial class Form1 : Form
    {
        [DllImport(@"md5.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "md5")]
        public static extern IntPtr md5(String key);

        public Form1()
        {
            InitializeComponent();
            this.textBox2.Focus();
           // this.textBox1.Text = getHardDiskId().Trim();
        }

        public static String getHardDiskId()
        {
            //System.Man
            ////获取硬盘ID
            String HDid="";
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
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string hard_disk_id = this.textBox2.Text;
            this.textBox1.Text = Marshal.PtrToStringAnsi(md5(hard_disk_id));
        }
    }
}
