using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HandyUploadForm
{
    public partial class PictureAutoSizeForm : Form
    {
        public PictureAutoSizeForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public bool CenterCtr(Control ctr, bool isLR, bool isUD)
        {
            Control pCtr = ctr.Parent;
            int x = isLR ? ((pCtr.Width - ctr.Width) / 2) : ctr.Location.X;
            int y = isUD ? ((pCtr.Height - ctr.Height) / 2) : ctr.Location.Y;
            ctr.Location = new System.Drawing.Point(x, y);
            return true;
        }

        private void PicutreAutoSizeForm_Shown(object sender, EventArgs e)
        {
            
            try
            {
                //pictureBox1.Load("http://hovertree.com/hvtimg/bjafjc/rgevo2ea.jpg");
               // pictureBox1.Load(@"C:\Users\Leo\Desktop\廖宸宇\psc.jfif");
            }
            catch (Exception ex) { MessageBox.Show("何问起", ex.Message); }
        }
    }
}
