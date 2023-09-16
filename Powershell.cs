using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace EzTweak
{
    public static class Powershell
    {
        public static string Start(string command, bool quiet = false)
        {
            var script = $"{command} | Out-String";
            PowerShell ps = PowerShell.Create();

            if (!quiet)
            {
                Log.WriteLine(command);
            }

            Collection<PSObject> results = ps.AddScript(script).Invoke();
            var output = string.Join(Environment.NewLine, results.Select(o => o.ToString()).ToList());
            return output;
        }
    }
}
