using PanaceaLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PanaceaLauncher
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            CheckForApps.IsChecked = App.Enabled;
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Processesutton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer");
            }
            catch
            {
            }
        }

        private void CheckForApps_OnChecked(object sender, RoutedEventArgs e)
        {
            App.Enabled = true;
        }

        private void CheckForApps_OnUnchecked(object sender, RoutedEventArgs e)
        {
            App.Enabled = false;
            
        }

        private void SystemSetup_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Common.Path() + "../SystemSetup/SystemSetup.exe");
            }
            catch
            {
            }
        }

        private void Cmd_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

                Process.Start("cmd.exe");

            }
            catch
            {
            }
        }

        private void LockButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.LockFileSystem();
        }

        private void UnlockButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.UnLockFileSystem();
        }
        private void Powershell_OnClick(object sender, RoutedEventArgs e)
        {
            try { Process.Start("powershell.exe"); }
            catch { }
        }
        public bool Is64bit { get; set; } = System.Environment.Is64BitOperatingSystem;
        private void Cmd64_OnClick(object sender, RoutedEventArgs e)
        {
            string sysNativePath = Environment.ExpandEnvironmentVariables(@"%windir%\Sysnative\cmd.exe");
            if (System.IO.File.Exists(sysNativePath))
            {
                try { Process.Start(sysNativePath); }
                catch { }
            }
        }
        private void Powershell64_OnClick(object sender, RoutedEventArgs e)
        {
            string sysNativePath = Environment.ExpandEnvironmentVariables(@"%windir%\Sysnative\WindowsPowerShell\v1.0\powershell.exe");
            if (System.IO.File.Exists(sysNativePath))
            {
                try { Process.Start(sysNativePath); }
                catch { }
            }
        }
        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = await TerminalIdentification.TerminalIdentificationManager.GetIdentificationInfoAsync();
                if(info == null)
                {
                    putikText.Text = "PUTIK not available";
                    return;

                }
                putikText.Text = info.Putik;
            }
            catch {
                putikText.Text = "<error>";
            }
        }
    }

    public static class DictExtensions
    {
        public static string ToString(this Dictionary<string, string> source, string keyValueSeparator,
            string sequenceSeparator)
        {
            if (source == null)
                throw new ArgumentException("Parameter source can not be null.");

            var pairs = source.Select(x => string.Format("{0}{1}{2}", x.Key, keyValueSeparator, x.Value)).ToArray();

            return string.Join(sequenceSeparator, pairs);
        }
    }
}
