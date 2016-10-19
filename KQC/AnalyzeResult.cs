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
using System.Drawing;
using System.Windows.Forms;
using System.Reactive.Linq;
using KQC.Backend;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace KQC
{
    public partial class AnalyzeResult : Form
    {
        string name;
        IObservable<EVE.Message> s;

        public AnalyzeResult(string _name)
        {
            name = _name;
            InitializeComponent();
#if DEBUG
            this.Text = pNameLabel.Text = "Sample Bad Guy";
            s = EVE.dummyCheckSource();
#else
            s = EVE.fullCheckSource(name).Publish().RefCount();
            this.Text = name;
            pNameLabel.Text = name;
#endif

            judgeTextBox.Text = "JUDGING";
            judgeTextBox.ForeColor = Color.White;
            judgeTextBox.BackColor = Color.LightBlue;
        }

        private void AnalyzeResult_Shown(object sender, EventArgs e)
        {
            var p = s.SubscribeOn(NewThreadScheduler.Default)
                     .ObserveOn(SynchronizationContext.Current);

            p.OfType<EVE.Message.Kos>()
             .Select(x => x.Item)
             .Subscribe(k =>
             {
                 if (k.IsNotFound)
                     listView1.Items.Add(new ListViewItem(new[] { EVE.getName(k), "Not found" }));
                 else
                     listView1.Items.Add(new ListViewItem(new[] { EVE.getName(k) + " (" + EVE.getType(k) + ")", EVE.isKos(k) ? "KOS" : "Not KOS" }));
             });

            p.OfType<EVE.Message.Text>()
             .Select(x => x.Item)
             .Subscribe(x =>
             {
                 detailTextBox.Text += x + Environment.NewLine;
             });

            p.OfType<EVE.Message.CharaIcon>()
             .Select(x => x.Item)
             .Subscribe(x =>
             {
                 pictureBox1.LoadAsync(x);
             });

            p.OfType<EVE.Message.Jud>()
             .Select(x => x.Item)
             .Subscribe(j =>
             {
                 if (j.IsThreat)
                 {
                     judgeTextBox.Text = "THREAT";
                     judgeTextBox.ForeColor = Color.White;
                     judgeTextBox.BackColor = Color.Black;
                 }
                 else if (j.IsDanger)
                 {
                     judgeTextBox.Text = "DANGER";
                     judgeTextBox.ForeColor = Color.White;
                     judgeTextBox.BackColor = Color.Red;
                 }
                 else if (j.IsCaution)
                 {
                     judgeTextBox.Text = "CAUTION";
                     judgeTextBox.ForeColor = Color.Black;
                     judgeTextBox.BackColor = Color.Yellow;
                 }
                 else if (j.IsSafe)
                 {
                     judgeTextBox.Text = "SAFE";
                     judgeTextBox.ForeColor = Color.White;
                     judgeTextBox.BackColor = Color.Green;
                 }
                 else if (j.IsNoInformation)
                 {
                     judgeTextBox.Text = "NO INFO";
                     judgeTextBox.ForeColor = Color.White;
                     judgeTextBox.BackColor = Color.Blue;
                 }
             });

            
        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listView1.Columns[e.ColumnIndex].Width;
        }
    }
}
