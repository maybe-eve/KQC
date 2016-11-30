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
using static KQC.Backend.Tactical;
using System.Threading;
using System.Threading.Tasks;

namespace KQC
{
    internal struct Ship
    {
        public string Name;
        public int Id;
        public double Kills;
        public double Losses;
        public double UsedPercent;
        public IEnumerable<Tuple<Tuple<string, int>, int>> Mods;
        public IEnumerable<Tuple<Tuple<string, int>, double>> UsedWeapons;
    }

    public partial class TacticalAnalyser : Form
    {
        IObservable<Tactical.Message> s;
        Dictionary<string, Ship> dict;

        int id;

        public TacticalAnalyser(int _id, string name = null)
        {
            id = _id;
            dict = new Dictionary<string, Ship>();
            InitializeComponent();

            if (name == null)
                this.Text = "TacticalAnalyser - " + EVE.getCharaNameById(id);
            else
                this.Text = name;
            s = getKillDetail(id).Publish().RefCount();

            listView1.ColumnWidthChanged += Program.GenerateListLocker(listView1);
            listView2.ColumnWidthChanged += Program.GenerateListLocker(listView2);
        }

        private void TacticalAnalyser_Shown(object sender, EventArgs e)
        {
            
            var p = s.SubscribeOn(NewThreadScheduler.Default)
                     .ObserveOn(SynchronizationContext.Current);

            p.OfType<Tactical.Message.ShipInfo>()
             .Subscribe(xs =>
             {
                 label1.Visible = false;
                 var s = shipChart.Series[0];
                 foreach(var x in xs.Item)
                 {
                     var name = x.Item1.Item1;
                     var id = x.Item1.Item2;
                     var kr = x.Item2;
                     var dr = x.Item3;
                     if (double.IsNaN(kr))
                         kr = 0;
                     if (double.IsNaN(dr))
                         dr = 0;
                     dict.Add(name, new Ship()
                     {
                         Id = id,
                         Name = name,
                         Kills = kr,
                         Losses = dr,
                         UsedPercent = kr,
                         Mods = x.Item4,
                         UsedWeapons = x.Item5
                     });
                     shipsComboBox.Items.Add(name);
                     if (kr != 0)
                     {
                         var dp = s.Points.Add(kr);
                         dp.AxisLabel = string.Format("{0}{2}{1}%", name, kr, Environment.NewLine);
                     }
                     if (!s.Points.Any())
                     {
                         label1.Text = "NO DATA";
                         label1.Visible = true;
                         //var dp = s.Points.Add(100);
                     }
                 }
                 var path = new System.Drawing.Drawing2D.GraphicsPath();
                 path.AddEllipse(new Rectangle(0, 0, 110, 110));
                 pictureBox1.Region = new Region(path);
                 pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                 pictureBox1.LoadAsync(EVE.getIconUriById(id));
                
                 if (shipsComboBox.Items.Count > 0)
                 {
                     shipsComboBox.Enabled = listView2.Enabled = radioButton1.Enabled = radioButton2.Enabled = true;
                     label7.Visible = false;
                     shipsComboBox.SelectedIndex = 0;
                     shipsComboBox_SelectedIndexChanged(this, null);
                 }
                 else
                 {
                     label7.Text = "NO DATA";
                 }
             });

            p.OfType<Tactical.Message.Gang>()
             .Subscribe(xs =>
             {
                 foreach (var x in xs.Item)
                     listView1.Items.Add(new ListViewItem(new[] { x.Item1, x.Item2.ToString() }));
                 kosButton.Enabled = true;
             });

            p.OfType<Tactical.Message.TzInfo>()
             .Subscribe(xs =>
             {
                 if (xs.Item.All(x => x.Item2 == 0))
                 {
                     label5.Text = "NO DATA";
                     label5.Visible = true;
                 }
                 else
                 {
                     label5.Visible = false;
                     label6.Visible = true;
                     var s = tzChart.Series[0];
                     var ca = tzChart.ChartAreas[0];
                     ca.AxisX.Interval = 3;
                     ca.AxisX.IntervalOffset = 1;
                     ca.AxisX.Maximum = 25;
                     if (ca.AxisY.Interval < 1)
                         ca.AxisY.Interval = 1;
                     foreach (var x in xs.Item)
                     {
                         var t = s.Points.Add(x.Item2);
                         t.AxisLabel = x.Item1.ToString();
                     }
                 }
             });

        }

        private void shipsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateItemList(true);
        }

        void updateItemList(bool changed = false)
        {
            var n = shipsComboBox.Items[shipsComboBox.SelectedIndex] as string;
            var s = dict[n];
            if (changed)
            {
                label10.Text = string.Format(@"{0}% kills
{1}% losses", s.Kills, s.Losses);
                shipPictureBox.LoadAsync(EVE.getRenderUriById(s.Id));
            }
            listView2.Items.Clear();
            if (radioButton1.Checked)
            {
                label8.Text = @"Showing weapon usage of this
7 days, found in killmails.";
                listView2.Columns[1].Text = "%";
                foreach (var x in s.UsedWeapons.OrderByDescending(y => y.Item2))
                {
                    listView2.Items.Add(new ListViewItem(new[] { x.Item1.Item1, x.Item2.ToString() }));
                }
            }

            else if (radioButton2.Checked)
            {
                label8.Text = @"Showing amount of items
destroyed in last 7 days.";
                listView2.Columns[1].Text = "x";
                foreach (var x in s.Mods.OrderByDescending(y => y.Item2))
                {
                    listView2.Items.Add(new ListViewItem(new[] { x.Item1.Item1, x.Item2.ToString() }));
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton2.Checked == radioButton1.Checked)
                radioButton2.Checked = !radioButton1.Checked;
            updateItemList();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == radioButton1.Checked)
                radioButton1.Checked = !radioButton2.Checked;
            updateItemList();
        }

        private void kosButton_Click(object sender, EventArgs e)
        {
            kosButton.Text = "Checking...";
            kosButton.Enabled = false;
            var xs = listView1.Items.Cast<ListViewItem>().ToArray();
            new Task(() =>
            {
                foreach (var i in xs)
                {
                    var name = i.SubItems[0].Text;
                    if (KOS.checkByName(name).Any(KOS.isKos))
                    {
                        this.Invoke(new Action(() => i.BackColor = Color.Red));
                        break;
                    }

                    else
                        this.Invoke(new Action(() => i.BackColor = Color.Green));
                }
                this.Invoke(new Action(() =>
                {
                    kosButton.Text = "Completed";
                }));
            }).Start();
        }
    }
}
