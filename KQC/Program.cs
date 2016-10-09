
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
