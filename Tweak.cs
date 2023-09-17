using Hardware.Info;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace EzTweak
{
    public class Tk
    {
        public string name;
        public string description;
        public System.Action turn_on;
        public System.Action turn_off;
        public Action<string> activate_value;
        public Func<List<string>> valid_values;
        public Func<string> current_value;
        public Func<string> status;
        public Func<bool> is_on;
    }

    public class Container_Tweak : Tk
    {
        public Tk[] tweaks;

        public Container_Tweak(string name, string description, Tk[] tweaks)
        {
            this.name = name;
            this.description = description;
            this.tweaks = tweaks;
            this.turn_on = () => Array.ForEach(tweaks, t => t.turn_on());
            this.turn_off = () => Array.ForEach(tweaks, t => t.turn_off());
            this.activate_value = (value) => Array.ForEach(tweaks, t => t.activate_value(value));
            this.valid_values = null;
            this.current_value = () => is_on() ? "ON" : "OFF";
            this.status = () => $"Status: {current_value()}";
            this.is_on = () => tweaks.All(t => t.is_on());
        }
    }

    public class CMD_Tweak : Tk
    {
        public CMD_Tweak(string name, string description, string on, string off, string status_command, string current_regex, string is_on_regex)
        {
            this.name = name;
            this.description = description;
            this.turn_on = () => Cmd.Start(on);
            this.turn_off = () => Cmd.Start(off);
            this.activate_value = null;
            this.valid_values = null;
            this.current_value = () =>
            {
                var output = Cmd.Start(status_command, true);
                var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
                return match.Groups[1].Value;
            };
            this.status = () => $"[{current_value()}] CMD: {status_command}";
            this.is_on = () =>
        {
            if (is_on_regex == null) { return false; }
            var output = Cmd.Start(status_command, true);
            var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
            return match.Success;
        };
        }
    }

    public class BCDEDIT_Tweak : Tk
    {
        public BCDEDIT_Tweak(string name, string description, string property, string on_value, string off_value)
        {
            this.name = name;
            this.description = description;
            this.turn_on = () => activate_value(on_value);
            this.turn_off = () => activate_value(off_value);
            this.activate_value = (string value) =>
        {
            if (value == Registry.REG_DELETE)
            {
                Bcdedit.Delete(property);
            }
            else
            {
                Bcdedit.Set(property, value);
            }
        };
            this.current_value = () => Bcdedit.Query(property);
            this.status = () => $"BCDEDIT: {current_value()}";
            this.is_on = () => Bcdedit.Match(property, on_value);
        }
    }

    public class Powershell_Tweak : Tk
    {
        public Powershell_Tweak(string name, string description, string on_command, string off_command, string status_command, string current_regex, string is_on_regex)
        {
            this.name = name;
            this.description = description;
            this.turn_on = () => Start(on_command);
            this.turn_off = () => Start(off_command);
            this.current_value = () =>
        {
                var output = Start(status_command, true);
                var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
                return match.Groups[1].Value;
            };
            this.status = () => $"[{current_value()}] Powershell: {status_command}";
            this.is_on = () =>
        {
                var output = Start(status_command, true);
                var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
                return match.Success;
            };
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

    public class RegistryTweak : Tk
    {
        public RegistryTweak(string name, string description, TweakType type, string path, string on_value, string off_value)
        {
            this.name = name;
            this.description = description;
            this.turn_on = () => activate_value(sanitize(on_value,type));
            this.turn_off = () => activate_value(sanitize(off_value, type));
            this.activate_value = (string value) =>
            {
                Registry.Set(path, sanitize(value, type), (RegistryValueKind)type);
            };
            this.current_value = () =>
            sanitize(Registry.From(path, (RegistryValueKind)type), type);
            this.status = () => $"\"{path}\"={current_value()}";
            this.is_on = () =>
            current_value() == on_value;
        }

        protected static string sanitize(string value, TweakType type)
        {
            switch (type)
            {
                case TweakType.DWORD:
                case TweakType.SERVICE:
                    return Registry.From_DWORD(Registry.To_DWORD(value));
                case TweakType.REG_SZ:
                    return Registry.From_REG_SZ(Registry.To_REG_SZ(value));
                case TweakType.BINARY:
                    return Registry.From_BINARY(Registry.To_BINARY(value));
                default: throw new NotImplementedException();
            }
        }
    }

    public class ServiceTweak : RegistryTweak
    {
        public ServiceTweak(string name, string description, string service, string on_value, string off_value) : base(name, description, TweakType.SERVICE, ServiceTweak.registry_path(service), on_value, off_value)
        {
            this.status += () =>
        {
            var value = current_value();
            return $"{service} is {alias(value)}";
        };
        }

        private static string registry_path(string service)
        {
            return $@"HKLM\SYSTEM\CurrentControlSet\Services\{service}\Start";
        }

        private static string alias(string value)
        {
            if (sanitize("4", TweakType.DWORD) == value)
            {
                return "(Disabled)";
            }

            if (sanitize("3", TweakType.DWORD) == value)
            {
                return "(Manual)";
            }

            if (sanitize("2", TweakType.DWORD) == value)
            {
                return "(Automatic)";
            }
            return "";
        }
    }
}
