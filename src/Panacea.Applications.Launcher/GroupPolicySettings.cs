using LocalPolicy;
using Microsoft.Win32;

namespace PanaceaLauncher
{
    internal static class GroupPolicySettings
    {
        internal static void Lock()
        {
#if DEBUG
            return;
#endif
            try
            {
                var gpo = new ComputerGroupPolicyObject();
                using (var machine = gpo.GetRootRegistryKey(GroupPolicySection.User))
                {
                    using (
                        var terminalServicesKey =
                            machine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                    {
                        terminalServicesKey?.SetValue("NoViewOnDrive", 0x03FFFFFF, RegistryValueKind.DWord);
                    }
                    using (
                        var terminalServicesKey =
                            machine.CreateSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun")
                    )
                    {
                        terminalServicesKey?.SetValue("1", "iexplore.exe", RegistryValueKind.String);

                    }
                }
                gpo.Save();
            }
            catch
            {
            }
        }

        internal static void Unlock()
        {
#if DEBUG
            return;
#endif
            try
            {
                var gpo = new ComputerGroupPolicyObject();
                using (var machine = gpo.GetRootRegistryKey(GroupPolicySection.User))
                {
                    using (
                        var terminalServicesKey =
                            machine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                    {
                        terminalServicesKey?.SetValue("NoViewOnDrive", 0, RegistryValueKind.DWord);
                    }
                    using (
                        var terminalServicesKey =
                            machine.CreateSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun"))
                    {
                        terminalServicesKey?.SetValue("1", "", RegistryValueKind.String);

                    }
                }
                gpo.Save();
            }
            catch
            {
            }
        }
    }
}
