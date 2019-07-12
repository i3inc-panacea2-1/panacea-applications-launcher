using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace PanaceaLauncher
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        public bool PasswordProvided { get; set; }
        private void PasswordBlock_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBlock.Password == password)
            {
                this.PasswordProvided = true;
                this.Close();
            }
        }

        private string password;

        private void PasswordWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (var key = Registry.LocalMachine.OpenSubKey("Software", true))
            using (var panacea = key.CreateSubKey("Panacea"))
            {
                password = panacea.GetValue("Password", "P@n@ce@").ToString();
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PasswordBlock.Focus();
            }));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("c:\\WINDOWS\\sysnative\\osk.exe"))
                {
                    Process.Start("c:\\WINDOWS\\sysnative\\osk.exe");
                }
                else
                {
                    Process.Start("osk.exe");
                }
            }
            catch
            {
            }
        }
    }
}
