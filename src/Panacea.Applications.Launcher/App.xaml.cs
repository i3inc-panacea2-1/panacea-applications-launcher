using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Gma.UserActivityMonitor;
using Microsoft.Win32;
using PanaceaLib;
using Application = System.Windows.Application;
using TerminalIdentification;
using System.Windows.ApplicationExtensions;
using WindowsInput;

namespace PanaceaLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : SingleInstanceApp
    {

        public App() : base("PanaceaLauncher")
        {
            InitializeComponent();
        }

        public static bool Enabled = true;
        private SettingsWindow _settings;
        private PasswordWindow _pass;
        private bool _isSettingsWindowOpen;

        public static void LockFileSystem()
        {
            if (_lockFsDisabled) return;
            try
            {
                foreach (var proc in Process.GetProcessesByName("explorer"))
                {
                    proc.Kill();
                }
            }
            finally
            {

            }
            try
            {
                GroupPolicySettings.Lock();
            }
            catch
            {
            }

        }

        public static void UnLockFileSystem()
        {
            if (_lockFsDisabled) return;
            try
            {
                GroupPolicySettings.Unlock();
            }
            catch
            {
            }
            try
            {
                foreach (var proc in Process.GetProcessesByName("explorer"))
                {
                    proc.Kill();
                }
            }
            finally
            {

            }
        }

        private static bool _lockFsDisabled = false;

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
          
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
           
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("Software", true))
                using (var panacea = key.CreateSubKey("Panacea"))
                using (var launcher = panacea.CreateSubKey("Launcher"))
                {
                    var disabled = (int) launcher.GetValue("Disabled", 0);
                    if (disabled == 1) Enabled = false;
                    _lockFsDisabled = (int) launcher.GetValue("LockFsDisabled", 0) == 1;
                }
            }
            catch
            {
            }
            LockFileSystem();
            _settings=new SettingsWindow();
            _settings.Closing += (s, args) =>
            {
                _isSettingsWindowOpen = false;
            };
            int _showDev = 0;
            HookManager.KeyDown += (o, ee) =>
            {
                if (!InputSimulator.IsKeyDown(VirtualKeyCode.LCONTROL) || !InputSimulator.IsKeyDown(VirtualKeyCode.VK_Q))
                    return;
                if (ee.KeyCode == Keys.D0 && _showDev == 0)
                {
                    ee.Handled = true;
                    _showDev = 1;
                    return;
                }
                if (ee.KeyCode == Keys.D9 && _showDev == 1)
                {
                    ee.Handled = true;
                    _showDev = 2;
                    return;
                }
                if (ee.KeyCode == Keys.D8 && _showDev == 2)
                {
                    _showDev = 0;
                    ee.Handled = true;
                    ShowUI();
                    return;
                }
                _showDev = 0;
            };

            var appsToCheck = new Dictionary<string, Dictionary<string, string>>();
            int registrySkipReadCounter = 7;
            if(e.Args.Contains("/show")) ShowUI();
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
	                    try
	                    {
		                    await Task.Delay(1200);

		                    if (!Enabled) continue;

                            if (registrySkipReadCounter > 6)
                            {
                                registrySkipReadCounter = 0;
                                appsToCheck = GetAppsToCheck();
                            }
                            else registrySkipReadCounter++;
                           
		                    foreach (var group in appsToCheck.Keys)
		                    {
			                    bool running = CheckGroupOfApps(appsToCheck[group]);
			                    if (!running)
			                    {
				                    await Task.Delay(4000); //let some time just to make sure
				                    running = CheckGroupOfApps(appsToCheck[group]);
			                    }
			                    if (!Enabled || running) continue;

			                    try
			                    {
                                    var g = group;
                                    var modules = (from app in appsToCheck[g].Keys
                                                   where !string.IsNullOrEmpty(app)
                                                   let process = Process.GetProcessesByName(app).FirstOrDefault()
                                                   where process != null

                                                   select $"{app}: {string.Join(", ", process.Modules)}");
                                    Log(
					                    $"No application found for group '{group}'. Launching default '{appsToCheck[@group][""]}'. {string.Join(Environment.NewLine, modules)}", EventLogEntryType.Warning);
				                    Process.Start(Common.Path() + "../../" + appsToCheck[group][""]);
			                    }
			                    catch (Exception ex)
			                    {
				                    Log(String.Format("Failed to launch '{0}'. '{1}'", appsToCheck[group][""], ex.Message),
					                    EventLogEntryType.FailureAudit);
			                    }
		                    }
	                    }
	                    catch(Exception ex){
							Log(String.Format("Exception '{0}'", ex.Message), EventLogEntryType.Warning);
	                    }
                    }
                });
            }
            catch
            {
                //closed
            }
        }

        void ShowUI()
        {
            if (!_isSettingsWindowOpen && _pass == null)
            {
                _pass = new PasswordWindow();
                _pass.Closed += (s, args) =>
                {
                    if (_pass.PasswordProvided)
                    {
                        _settings.Show();
                        _isSettingsWindowOpen = true;
                        _settings.Activate();
                    }
                    _pass = null;
                };
                _pass.Show();
                _pass.Activate();
                
            }
            else if (_pass != null)
            {
                _pass.Activate();
            }
            else
            {
                _settings.Activate();
            }
        }

        private static bool CheckGroupOfApps(Dictionary<string, string> apps)
        {
            var pathInfo = new DirectoryInfo(Common.Path()).Parent.Parent.Parent;
            var path = pathInfo.FullName;

            return (from app in apps.Keys
                where !string.IsNullOrEmpty(app) 
                && Process.GetProcessesByName(app)
                .Any(p => p.Modules.Cast<ProcessModule>().Any(m => m.FileName == Path.Combine(path, apps[app])))
                select app).Any();
        }

        internal static Dictionary<string, Dictionary<string, string>> GetAppsToCheck()
        {
            var appsToCheck = new Dictionary<string, Dictionary<string, string>>();
            using (var key = Registry.LocalMachine.OpenSubKey("Software", true))
            using (var panacea = key.CreateSubKey("Panacea"))
            using (var launcher = panacea.CreateSubKey("Launcher"))
            using (var processes = launcher.CreateSubKey("Processes"))
            {
                foreach (var groupName in processes.GetSubKeyNames())
                {
                    appsToCheck.Add(groupName, new Dictionary<string, string>());
                    using (var group = processes.CreateSubKey(groupName))
                    {
                        foreach (var subkey in group.GetValueNames())
                        {
                            try
                            {
                                var app = (string) group.GetValue(subkey, null);
                                if (app != null)
                                    appsToCheck[groupName].Add(subkey, app);
                            }
                            catch
                            {
                                //ignore
                            }
                        }
                    }
                }
                return appsToCheck;
            }
        }
        
        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            unobservedTaskExceptionEventArgs.SetObserved();
            Exception ex =
                unobservedTaskExceptionEventArgs.Exception;
            Log(ex.Message, EventLogEntryType.Error);
            if(ex.InnerException !=null)
                Log(ex.InnerException.Message, EventLogEntryType.Error);

            if (Current.Dispatcher != null)
                Current.Dispatcher.Invoke(() => Current.Shutdown());

            Process.Start(Common.Path() + "PanaceaLauncher.exe");
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                Current.Shutdown();
                Process.Start(Common.Path() + "PanaceaLauncher.exe");
            }
            catch
            {
            }
        }

        private void Log(string message, EventLogEntryType type)
        {
	        try
	        {
		        EventLog appLog = new EventLog
		        {
			        Source = "PanaceaLauncher"
		        };
		        appLog.WriteEntry(message, type, 0);
	        }
	        catch
	        {
		        
	        }
        }

        public override bool SignalExternalCommandLineArgs(IList<string> args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (args.Contains("/show"))
                {
                    ShowUI();
                }
            }));
            return true;
        }
    }
}
