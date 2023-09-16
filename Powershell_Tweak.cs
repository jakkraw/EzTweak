using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace EzTweak
{
    public class Powershell_Tweak : Tk
    {
        string on_command;
        string off_command;
        string status_command;
        string current_regex;
        string is_on_regex;

        public Powershell_Tweak(string name, string description, string on_command, string off_command, string status_command,string current_regex, string is_on_regex) : base(name, description, ActionType.POWERSHELL)
        {
            this.on_command = on_command;
            this.off_command = off_command;
            this.status_command = status_command;
            this.current_regex = current_regex;
            this.is_on_regex = is_on_regex;
        }

        public override void turn_off()
        {
            Start(off_command);
        }

        public override void turn_on()
        {
            Start(on_command);
        }

        public override void activate_value(string value)
        {
            throw new NotImplementedException();
        }

        public override string status()
        {
            return $"[{current_value()}] Powershell: {status_command}";
        }

        public override string current_value()
        {
            var output = Start(status_command, true);
            var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
            return match.Groups[1].Value;
        }

        public override bool is_on()
        {
            var output = Start(status_command, true);
            var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
            return match.Success;
        }

        public override List<string> valid_values()
        {
            throw new NotImplementedException();
        }


        private static string Start(string command, bool quiet = false)
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
