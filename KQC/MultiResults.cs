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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using KQC.Backend;
using System.Threading;
using System.Threading.Tasks;

namespace KQC
{
    public partial class MultiResults : Form
    {

        string[] names;

        public MultiResults(string[] _names)
        {
            InitializeComponent();
            names = _names;
        }

        private void MultiResults_Load(object sender, EventArgs e)
        {
            new Task(() =>
            {
                foreach (var n in names)
                {
                    this.Invoke(new Action(() =>
                    {
                        var f = new AnalyzeResult(n);
                        f.TopLevel = false;
                        var tp = new TabPage(n);
                        f.FormClosing += (_, __) =>
                        {
                            f.Dispose();
                            tp.Controls.Clear();
                            tabControl1.TabPages.Remove(tp);
                            tabControl1.Update();
                            if (tabControl1.TabCount == 0)
                                this.Close();
                        };
                        f.panel1.ControlAdded += tabControl1_SelectedIndexChanged;
                        f.panel1.VisibleChanged += tabControl1_SelectedIndexChanged;
                        tp.Controls.AddRange(f.Controls.Cast<Control>().ToArray());
                        tabControl1.TabPages.Add(tp);
                        f.Show();
                    }));
                    Thread.Sleep(500);
                }
            }).Start();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var p = tabControl1.SelectedTab.Controls["panel1"];
                if (p.Enabled && p.Visible)
                    this.Size = new Size(276 + 313, 423);
                else
                    this.Size = new Size(276, 423);
            }
            catch { }
        }

        private void MultiResults_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var t in tabControl1.TabPages.Cast<TabPage>())
                t.Controls.Clear();
            tabControl1.TabPages.Clear();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var tp = tabControl1.SelectedTab;
                tp.Controls.Clear();
                tabControl1.TabPages.Remove(tp);
                tabControl1.Update();
                if (tabControl1.TabCount == 0)
                    this.Close();
            }
            catch { }
        }
    }
}
