/*
KQC - KOS Quick Checker
Copyright (c) 2016 maybe-eve
This file is part of KQC.
KQC is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

        string text = "";

        public MainForm()
        {
            InitializeComponent();
            if (!Program.IsMono)
            {
                var i = Clipboard.GetDataObject();
                if (i.GetDataPresent(DataFormats.Text))
                {
                    var _text = (string)i.GetData(DataFormats.Text);
                    ReceiveClipboard(_text);
                }
                AddClipboardFormatListener(this.Handle);
            }
            else
            {
                this.Activated += (object sender, EventArgs e) => 
                    {
                        var i = Clipboard.GetDataObject();
                        if (i.GetDataPresent(DataFormats.Text)) {
                            var _text = (string)i.GetData(DataFormats.Text);
                            ReceiveClipboard(_text);
                        }  
                    };
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_CLIPBOARDUPDATE) {
                var i = Clipboard.GetDataObject();
                if (i.GetDataPresent(DataFormats.Text)) {
                    var _text = (string)i.GetData(DataFormats.Text);
                    ReceiveClipboard(_text);
                } 
            }
        }

        void ReceiveClipboard(string _text)
        {
            text = _text;
            if (text.Contains("\n"))
                button1.Text = "(Multiple Pilots)";
            else
                button1.Text = text; 
        }
        
        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Program.IsMono)
            {
                RemoveClipboardFormatListener(this.Handle);
            }
        }
        
        string toKos(bool b)
        {
            return b ? "KOS" : "Not KOS";
        }
        
        void Button1Click(object sender, EventArgs e)
        {
            var a = text.Contains("\n") 
                ? (Form)new MultiResults(text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)) 
                : (Form)new AnalyzeResult(text);
            a.StartPosition = FormStartPosition.Manual;
            a.Location = this.Location;
            a.Show();
        }

        private void settingButton_Click(object sender, EventArgs e)
        {
            new SettingWindow().ShowDialog();
        }
    }
}
