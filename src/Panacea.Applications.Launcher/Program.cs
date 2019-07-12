using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using PanaceaLib;

namespace PanaceaLauncher
{
    public class Program
    {
        private const string Unique = "PanaceaLauncher";

        [STAThread]
        public static void Main()
        {
	        try
	        {
		        new App().Run();
	        }
	        catch
	        {
	        }
        }

        
    }
}