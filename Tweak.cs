using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EzTweak
{
    public abstract class Tk
    {
        public string name;
        public string description;
        public TweakType type;

        protected Tk(string name, string description, TweakType type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
        }

        public string get_name()
        {
            return name;
        }
        public string get_description()
        {
            return description;
        }

        public abstract void turn_on();
        public abstract void turn_off();
        public abstract void activate_value(string value);
        public abstract List<string> valid_values();

        public abstract string current_value();
        public abstract string status();
        public abstract bool is_on();
    }

    public class Container_Tweak : Tk
    {
        public Tk[] tweaks;

        public Container_Tweak(string name, string description, Tk[] tweaks) : base(name, description, TweakType.TWEAKS)
        {
            this.tweaks = tweaks;
        }

        public override void turn_on()
        {
            Array.ForEach(tweaks, t => t.turn_on());   
        }
        public override void turn_off()
        {
            Array.ForEach(tweaks, t => t.turn_off());
        }
        public override void activate_value(string value)
        {
            Array.ForEach(tweaks, t => t.activate_value(value));
        }
        public override List<string> valid_values()
        {
            return null;
        }

        public override string current_value()
        {
            return is_on() ? "ON" : "OFF";
        }
        public override string status()
        {
            return $"Status: {current_value()}";
        }

        public override bool is_on()
        {
            return tweaks.All(t => t.is_on());
        }
    }

    public class Tweak
    {
        public string name;
        public string description;
        public System.Action off_func;
        public System.Action on_func;
        public Action<string> set_func;
        public Regex on_regex;
        public string lookup_func;
        public string lookup_prefix;
        public Func<bool> is_on;
        public Func<string> lookup;
        public string on_description;
        public string off_description;
        public Dictionary<string, string> available_values;
        public string value;
        public List<Tweak> tweaks = new List<Tweak> { };


        public Tweak()
        {
        }

        public static Tweak DeviceDisable(Device device)
        {
            var name = "Disable Driver";
            var description = $"Disable {device.Name}{Environment.NewLine}{Environment.NewLine}{device.FullInfo}";
            var status_command = $"pnputil /enum-devices /instanceid \"{device.PnpDeviceID}\" | findstr /c:\"Status:\"";
            var status_regex = "Status:.*";
            var is_on_regex = "Status:.*Disabled";
            var on_command = $"pnputil /disable-device \"{device.PnpDeviceID}\"";
            var off_command = $"pnputil /enable-device \"{device.PnpDeviceID}\"";

            var tk = new CMD_Tweak(name, description, on_command, off_command, status_command, status_regex, is_on_regex);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.CMD(off_command, on_command, status_command, status_regex, is_on_regex));
            return tweak;
        }

        public static Tweak DeviceIdleRPIN(Device device)
        {
            var name = "Disable AllowIdleIrpInD3";
            var description = "Disable power saving option";
            var path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\AllowIdleIrpInD3";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.REGISTRY_DWORD(path, off_value, on_value));

            return Registry.Get(path) == null ? null : tweak;
        }

        public static Tweak EnhancedPowerManagementEnabled(Device device)
        {
            var name = "Disable Enhanced Power Management";
            var description = "Disable Enhanced Power Management";
            var path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\EnhancedPowerManagementEnabled";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.REGISTRY_DWORD(path, off_value, on_value));

            return Registry.Get(path) == null ? null : tweak;
        }

        public static Tweak MsiSupported(Device device)
        {
            var name = "Enable MSI";
            var description = "Enable MSI";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            var path = $@"{base_path}\MSISupported";
            var type = TweakType.DWORD;
            var off_value = "0";
            var on_value = "1";

            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.REGISTRY_DWORD(path, off_value, on_value));

            return Registry.Get(base_path) == null ? null : tweak;
        }

        public static Tweak DevicePriority(Device device)
        {
            var name = "Device Priority High";
            var description = "Device Priority High";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\DevicePriority";
            var type = TweakType.DWORD;
            var off_value = Registry.REG_DELETE;
            var on_value = "3";

            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.REGISTRY_DWORD(path, off_value, on_value));

            return Registry.Get(base_path) == null ? null : tweak;
        }

        public static Tweak AssignmentSetOverride(Device device)
        {
            var name = "Set Affinity";
            var description = "Set Affinity";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\AssignmentSetOverride";
            var type = TweakType.DWORD;
            var off_value = Registry.REG_DELETE;
            var on_value = "3F";

            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            Tweak tweak = new Tweak { };
            tweak.name = name;
            tweak.description = description;
            tweak.tweaks.Add(Tweak.REGISTRY_DWORD(path, off_value, on_value));

            return Registry.Get(base_path) == null ? null : tweak;
        }

        private static Tweak REGISTRY(string path, string off, string on, RegistryValueKind type)
        {
            Tweak action = new Tweak();
            action.name = path;
            action.lookup = () => { return Registry.From(path, type); };
            action.is_on = () => action.lookup() == on;

            if (off != null)
            {
                action.off_description = $"\"{path}\"=\"{off}\"";
                action.off_func = () => { Registry.Set(path, off, type); };
            }

            if (on != null)
            {
                action.on_description = $"\"{path}\"=\"{on}\"";
                action.on_func = () => { Registry.Set(path, on, type); };

            }
            return action;
        }

        public static Tweak REGISTRY_DWORD(string path, string off, string on)
        {
            return REGISTRY(path, Registry.From_DWORD(Registry.To_DWORD(off)), Registry.From_DWORD(Registry.To_DWORD(on)), RegistryValueKind.DWord);
        }

        public static Tweak REGISTRY_REG_SZ(string path, string off, string on)
        {
            return REGISTRY(path, Registry.From_REG_SZ(Registry.To_REG_SZ(off)), Registry.From_REG_SZ(Registry.To_REG_SZ(on)), RegistryValueKind.String);
        }

        public static Tweak REGISTRY_BINARY(string path, string off, string on)
        {
            return REGISTRY(path, Registry.From_BINARY(Registry.To_BINARY(off)), Registry.From_BINARY(Registry.To_BINARY(on)), RegistryValueKind.Binary);
        }

        public static Tweak SERVICE(string service, string off, string on)
        {
            return REGISTRY_DWORD($@"HKLM\SYSTEM\CurrentControlSet\Services\{service}\Start", off, on);
        }

        public static Tweak CMD(string off_cmd, string on_cmd, string lookup_cmd, string lookup_regex, string on_regex)
        {
            Tweak action = new Tweak();
            action.lookup_func = lookup_cmd;
            action.name = lookup_cmd;
            if (off_cmd != null)
            {
                action.off_func = () => Cmd.Start(off_cmd);
                action.off_description = $"[CMD] {off_cmd}";
            }

            if (lookup_cmd != null && lookup_regex != null)
            {
                action.lookup = () => Regex.Match(Cmd.Start(lookup_cmd, true), lookup_regex, RegexOptions.Multiline).Groups[1].Value;

                if (lookup_regex != null && on_regex != null)
                {
                    action.is_on = () => Regex.Match(Cmd.Start(lookup_cmd, true), on_regex, RegexOptions.Multiline).Success;
                }
            }

            if (on_cmd != null)
            {
                action.on_func = () => Cmd.Start(on_cmd);
                action.on_description = $"[CMD] {on_cmd}";
            }

            return action;
        }

        public static Tweak POWERSHELL(string off_cmd, string on_cmd, string lookup_cmd, string lookup_regex, string on_regex)
        {
            Tweak action = new Tweak();
            action.lookup_func = lookup_cmd;
            action.name = lookup_cmd;
            if (off_cmd != null)
            {
                action.off_func = () => Powershell.Start(off_cmd);
                action.off_description = $"[Powershell] {off_cmd}";
            }

            if (lookup_cmd != null)
            {
                action.lookup = () => Regex.Match(Powershell.Start(lookup_cmd, true), lookup_regex, RegexOptions.Multiline).Groups[1].Value;

                if (lookup_regex != null && on_regex != null)
                {
                    action.is_on = () => Regex.Match(Powershell.Start(lookup_cmd, true), on_regex, RegexOptions.Multiline).Success;
                }
            }

            if (on_cmd != null)
            {
                action.on_func = () => Powershell.Start(on_cmd);
                action.on_description = $"[Powershell] {on_cmd}";
            }

            return action;
        }

        public static Tweak BCDEDIT(string property, string value_off, string value_on)
        {
            Tweak action = new Tweak();

            var off_delete = value_off == Registry.REG_DELETE;
            var on_delete = value_on == Registry.REG_DELETE;

            action.lookup = () => Bcdedit.Query(property);
            action.name = property;

            if (value_off != null)
            {
                if (off_delete)
                {
                    action.off_func = () => Bcdedit.Delete(property);
                    action.off_description = $"bcdedit.exe /deletevalue {property}";
                }
                else
                {
                    action.off_func = () => Bcdedit.Set(property, value_off);
                    action.off_description = $"bcdedit.exe /set {property} {value_off}";
                }
            }

            if (value_on != null)
            {
                action.is_on = () => Bcdedit.Match(property, value_on);
                if (on_delete)
                {
                    action.on_func = () => Bcdedit.Delete(property);
                    action.on_description = $"bcdedit.exe /deletevalue {property}";
                }
                else
                {
                    action.on_func = () => Bcdedit.Set(property, value_on);
                    action.on_description = $"bcdedit.exe /set {property} {value_on}";
                }
            }

            return action;
        }

    }
}
