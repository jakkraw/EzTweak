using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace EzTweak
{
    public class WindowsSystem
    {
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsUserAnAdmin();

        public static bool IsTrustedInstaller()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var ti_sid = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
            return principal.IsInRole(new SecurityIdentifier(ti_sid));
        }

        public static void StartAsTrustedInstaller()
        {
            string[] args = Environment.GetCommandLineArgs();
            TrustedInstaller.StartAsChild(args);
        }

        public static void StartAsAdmin()
        {
            var proc = new Process
            {
                StartInfo =
            {
                FileName = Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas"
            }
            };

            proc.Start();
        }

        public static UserType GetUserType()
        {
            if (IsTrustedInstaller())
            {
                return UserType.TRUSTED_INSTALLER;
            }
            else if (IsUserAnAdmin())
            {
                return UserType.ADMIN;
            }
            else
            {
                return UserType.USER;
            }
        }



        public enum UserType
        {
            USER,
            ADMIN,
            TRUSTED_INSTALLER,
        }

    }
}
