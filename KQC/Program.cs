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
using System.IO;
using System.Diagnostics;

namespace KQC
{
    internal sealed class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            
            catch(Exception e)
            {
                MessageBox.Show("Sorry! An error occurred.\nPlease paste the log.txt to somewhere and let me know.\nI'll fix it in the next release as well as possible.");
                var p = Path.Combine(Application.ExecutablePath, "log.txt");
                File.WriteAllText(p, e.ToString());
                Process.Start(p);
            }
        }
        
    }
}
