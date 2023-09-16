using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EzTweak
{
    public class CMD_Tweak : Tk
    {
        string on_command;
        string off_command;
        string status_command;
        string current_regex;
        string is_on_regex;

        public CMD_Tweak(string name, string description, string on, string off, string status_command, string current_regex, string is_on_regex) : base(name,description, TweakType.CMD)
        {
            this.on_command = on;
            this.off_command = off;
            this.status_command = status_command;
            this.current_regex = current_regex;
            this.is_on_regex = is_on_regex;
        }

        public override void turn_off()
        {
            Cmd.Start(off_command);
        }

        public override void turn_on()
        {
            Cmd.Start(on_command);
        }

        public override void activate_value(string value)
        {
            throw new NotImplementedException();
        }

        public override string status()
        {
            return $"[{current_value()}] CMD: {status_command}";
        }

        public override string current_value()
        {
            var output = Cmd.Start(status_command, true);
            var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
            return match.Groups[1].Value;
        }

        public override bool is_on()
        {
            if (is_on_regex == null) { return false; }
            var output = Cmd.Start(status_command, true);
            var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
            return match.Success;
        }

        public override List<string> valid_values()
        {
            throw new NotImplementedException();
        }
    }
}
