using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EzTweak
{
    internal static class EzTweak
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1  &&!IsTrustedInstaller())
            {
                TrustedInstaller.StartAsChild(args);
                return;
            }
           

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new App());
        }


        public static bool IsTrustedInstaller()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var ti_sid = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
            return principal.IsInRole(new SecurityIdentifier(ti_sid));
        }
    }

}