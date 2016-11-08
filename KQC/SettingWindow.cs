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
using System.Windows.Forms;

namespace KQC
{
    public partial class SettingWindow : Form
    {
        public SettingWindow()
        {
            InitializeComponent();
            checkBox1.Checked = Properties.Settings.Default.SafeAutoClose;
            checkBox2.Checked = Properties.Settings.Default.DangerAutoAnalyse;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SafeAutoClose = checkBox1.Checked;
            Properties.Settings.Default.DangerAutoAnalyse = checkBox2.Checked;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
