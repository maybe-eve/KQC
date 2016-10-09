
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace KQC
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddClipboardFormatListener(IntPtr hwnd);
        
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        
        public MainForm()
        {
            InitializeComponent();
            AddClipboardFormatListener(this.Handle);   
            
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_CLIPBOARDUPDATE) 
            {
                var i = Clipboard.GetDataObject();
                if (i.GetDataPresent(DataFormats.Text)) 
                {
                    var text = (string)i.GetData(DataFormats.Text);
                    button1.Text = text;
                } 
            }
        }
        
        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveClipboardFormatListener(this.Handle);
        }
        
        string toKos(bool b)
        {
            return b ? "KOS" : "Not KOS";
        }
        
        void Button1Click(object sender, EventArgs e)
        {
            var pn = button1.Text.Replace(' ', '+');
            var u = string.Format("http://kos.cva-eve.org/api/?c=json&type=unit&q={0}", pn);
            var wr = WebRequest.Create(u);
            var r = "";
            using(var rs = wr.GetResponse())
                using(var st = rs.GetResponseStream())
                    using(var sr = new StreamReader(st, Encoding.UTF8))
                        r = sr.ReadToEnd();
            dynamic ro = JsonConvert.DeserializeObject(r);
            
            if(ro.results.Count == 0)
            {
                MessageBox.Show("No Result Found", button1.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var plyr = (bool)ro.results[0].kos;
            var corp = (bool)ro.results[0].corp.kos;
            var ally = (bool)ro.results[0].corp.alliance.kos;
            if(plyr || corp || ally)
            {
                MessageBox.Show(string.Format("KOS\n\nPlayer:   {0}\nCorp:     {1}\nAlliance: {2}", toKos(plyr), toKos(corp), toKos(ally)), button1.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Not KOS", button1.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
