using System;
using System.Windows.Forms;

namespace PigFarm.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Phải gọi TRƯỚC ApplicationConfiguration.Initialize()
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}