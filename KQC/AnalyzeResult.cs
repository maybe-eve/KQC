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
using static KQC.Backend.KOS;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace KQC
{
    public partial class AnalyzeResult : Form
    {
        string name;
        IObservable<KOS.Message> s;
        int id;

        public AnalyzeResult(string _name)
        {
            name = _name.Trim();
            InitializeComponent();
            s = fullCheckSource(name).Publish().RefCount();
            this.Text = name;
            pNameLabel.Text = name;
            judgeTextBox.Text = "JUDGING";
            judgeTextBox.ForeColor = Color.White;
            judgeTextBox.BackColor = Color.LightBlue;
            listView1.ColumnWidthChanged += Program.GenerateListLocker(listView1);
        }

        private void AnalyzeResult_Shown(object sender, EventArgs e)
        {
            var p = s.SubscribeOn(NewThreadScheduler.Default)
                     .ObserveOn(SynchronizationContext.Current);

            p.OfType<KOS.Message.Id>()
             .Select(x => x.Item)
             .Subscribe(x =>
             {
                 id = x;
             });

            p.OfType<KOS.Message.Kos>()
             .Select(x => x.Item)
             .Subscribe(k =>
             {
                 if (k.IsNotFound)
                     listView1.Items.Add(new ListViewItem(new[] { getName(k), "Not found" }));
                 else
                     listView1.Items.Add(new ListViewItem(new[] { getName(k) + " (" + getType(k) + ")", isKos(k) ? "KOS" : "Not KOS" }));
             }, Program.FailWith);

            p.OfType<KOS.Message.Text>()
             .Select(x => x.Item)
             .Subscribe(x =>
             {
                 detailTextBox.Text += x + Environment.NewLine;
             }, Program.FailWith);

            p.OfType<KOS.Message.CharaIcon>()
             .Select(x => x.Item)
             .Subscribe(x =>
             {
                 pictureBox1.LoadAsync(x);
             }, Program.FailWith);

            p.OfType<KOS.Message.Jud>()
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

                 if (!j.IsNoInformation)
                     tactButton.Enabled = true;

                 if (j.IsSafe && Properties.Settings.Default.SafeAutoClose)
                     new Task(() =>
                     {
                         Thread.Sleep(2000);
                         this.Invoke(new Action(this.Dispose));
                         this.Invoke(new Action(this.Close));
                     }).Start();
                 
                 else if ((j.IsDanger || j.IsThreat) && Properties.Settings.Default.DangerAutoAnalyse)
                     tactButton_Click(this, EventArgs.Empty);
             }, Program.FailWith);   
        }

        private void tactButton_Click(object sender, EventArgs e)
        {
            new TacticalAnalyser(id, name).Show();
        }
    }
}
