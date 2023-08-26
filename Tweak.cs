using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EzTweak {
    public class TweakAction {
        public string name;
        public string description;
        public Action off_func;
        public Action on_func;
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
        public void setValue(string value) {
            this.value = value;
        }

        public static TweakAction REGISTRY(string path, string off, string on, RegistryValueKind type) {
            TweakAction action = new TweakAction();
            action.lookup = () => { return Registry.Get(path, type); };
            action.is_on = () => action.lookup() == on;

            if (off != null) {
                var delete = off == Registry.REG_DELETE;
                action.off_description = $"\"{path}\"{(delete ? " 🗑" : $"=\"{off}\"")}";
                action.off_func = () => { if (delete) Registry.Delete(path); else Registry.Set(path, off, type); };
            }

            if (on != null) {
                var delete = on == Registry.REG_DELETE;
                action.on_description = $"\"{path}\"{(delete ? " 🗑" : $"=\"{on}\"")}";
                action.on_func = () => { if (delete) Registry.Delete(path); else Registry.Set(path, on, type); };

            }

            return action;
        }

        public static TweakAction REGISTRY_MULTI(string path, RegistryValueKind type) {
            TweakAction action = new TweakAction();
            action.lookup = () => { return Registry.Get(path, type); };
            var delete = action.value == Registry.REG_DELETE;
            action.off_description = $"\"{path}\"{(delete ? " 🗑" : $"=\"{action.value}\"")}";
            action.set_func = (v) => { if (delete) Registry.Delete(path); else Registry.Set(path, action.value, type); };
            return action;
        }

        public static TweakAction SERVICE(string service, string off, string on) {
            return REGISTRY($@"HKLM\SYSTEM\CurrentControlSet\Services\{service}\Start", off, on, RegistryValueKind.DWord);
        }

        public static TweakAction CMD(string off_cmd, string on_cmd, string lookup_cmd = null, string lookup_regex = null, string on_regex = null) {
            TweakAction action = new TweakAction();
            action.lookup_func = lookup_cmd;
            if (off_cmd != null) {
                action.off_func = () => Cmd.Start(off_cmd);
                action.off_description = $"[CMD] {off_cmd}";
            }

            if (lookup_cmd != null && lookup_regex != null) {
                action.lookup = () => Regex.Match(Cmd.Start(lookup_cmd, true), lookup_regex, RegexOptions.Multiline).Value;

                if (lookup_regex != null && on_regex != null) {
                    action.is_on = () => Regex.Match(action.lookup(), on_regex, RegexOptions.Multiline).Success;
                }
            }

            if (on_cmd != null) {
                action.on_func = () => Cmd.Start(on_cmd);
                action.on_description = $"[CMD] {on_cmd}";
            }

            return action;
        }

        public static TweakAction POWERSHELL(string off_cmd, string on_cmd, string lookup_cmd = null, string lookup_regex = null, string on_regex = null) {
            TweakAction action = new TweakAction();
            action.lookup_func = lookup_cmd;
            if (off_cmd != null) {
                action.off_func = () => Powershell.Start(off_cmd);
                action.off_description = $"[Powershell] {off_cmd}";
            }

            if (lookup_cmd != null) {
                action.lookup = () => Regex.Match(Powershell.Start(lookup_cmd), lookup_regex, RegexOptions.Multiline).Value;

                if (lookup_regex != null && on_regex != null) {
                    action.is_on = () => Regex.Match(action.lookup(), on_regex, RegexOptions.Multiline).Success;
                }
            }

            if (on_cmd != null) {
                action.on_func = () => Powershell.Start(on_cmd);
                action.on_description = $"[Powershell] {on_cmd}";
            }

            return action;
        }

        public static TweakAction BCDEDIT(string property, string value_off, string value_on) {
            TweakAction action = new TweakAction();

            var off_delete = value_off == Registry.REG_DELETE;
            var on_delete = value_on == Registry.REG_DELETE;

            action.lookup = () => $"{property} {Bcdedit.Query(property)}";

            if (value_off != null) {
                if (off_delete) {
                    action.off_func = () => Bcdedit.Delete(property);
                    action.off_description = $"bcdedit.exe /deletevalue {property}";
                } else {
                    action.off_func = () => Bcdedit.Set(property, value_off);
                    action.off_description = $"bcdedit.exe /set {property} {value_off}";
                }
            }

            if (value_on != null) {
                action.is_on = () => Bcdedit.Match(property, value_on);
                if (on_delete) {
                    action.on_func = () => Bcdedit.Delete(property);
                    action.on_description = $"bcdedit.exe /deletevalue {property}";
                } else {
                    action.on_func = () => Bcdedit.Set(property, value_on);
                    action.on_description = $"bcdedit.exe /set {property} {value_on}";
                }
            }

            return action;
        }

    }

    public class Tweak {
        public string name;
        public string description;
        public List<TweakAction> actions = new List<TweakAction>();

        public static Tweak DeviceDisable(Device device) {
            Tweak tweak = new Tweak { };
            tweak.name = "Disable Driver";
            tweak.description = $"Disable {device.Name}{Environment.NewLine}{Environment.NewLine}{device.FullInfo}";
            var lookup_cmd = $"pnputil /enum-devices /instanceid \"{device.PnpDeviceID}\" | findstr /c:\"Status:\"";
            var lookup_regex = "Status:.*";
            var on_regex = "Status:.*Disabled";
            var on_cmd = $"pnputil /disable-device \"{device.PnpDeviceID}\"";
            var off_cmd = $"pnputil /enable-device \"{device.PnpDeviceID}\"";
            tweak.actions.Add(TweakAction.CMD(off_cmd, on_cmd, lookup_cmd, lookup_regex, on_regex));
            return tweak;
        }

        public static Tweak DeviceIdleRPIN(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var AllowIdleIrpInD3 = $@"{reg}\AllowIdleIrpInD3";
            if (Registry.Get(AllowIdleIrpInD3) != null) {
                Tweak tweak = new Tweak { };
                tweak.name = "Disable AllowIdleIrpInD3";
                tweak.description = $"Disable power saving option";
                tweak.actions.Add(TweakAction.REGISTRY(AllowIdleIrpInD3, "1", "0", RegistryValueKind.DWord));
                return tweak;
            } else { return null; }
        }

        public static Tweak EnhancedPowerManagementEnabled(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var EnhancedPowerManagementEnabled = $@"{reg}\EnhancedPowerManagementEnabled";
            if (Registry.Get(EnhancedPowerManagementEnabled) != null) {
                Tweak tweak = new Tweak { };
                tweak.name = "Disable Enhanced Power Management";
                tweak.description = $"Disable Enhanced Power Management";
                tweak.actions.Add(TweakAction.REGISTRY(EnhancedPowerManagementEnabled, "1", "0", RegistryValueKind.DWord));
                return tweak;
            } else { return null; }
        }

        public static Tweak MsiSupported(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";

            if (Registry.Exists(reg_val)) {
                var MSISupported = $@"{reg_val}\MessageSignaledInterruptProperties\MSISupported";
                Tweak tweak = new Tweak { };
                tweak.name = "Enable MSI";
                tweak.description = $"Enable MSI";
                tweak.actions.Add(TweakAction.REGISTRY(MSISupported, "0", "1", Microsoft.Win32.RegistryValueKind.DWord));
                return tweak;
            } else { return null; }
        }

        public static Tweak DevicePriority(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";

            if (Registry.Exists(reg_val)) {
                var DevicePriority = $@"{reg_val}\Affinity Policy\DevicePriority";
                Tweak tweak = new Tweak { };
                tweak.name = "Device Priority High";
                tweak.description = $"Device Priority High";
                tweak.actions.Add(TweakAction.REGISTRY(DevicePriority, Registry.REG_DELETE, "3", RegistryValueKind.DWord));
                return tweak;
            } else { return null; }
        }

        public static Tweak AssignmentSetOverride(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";

            if (Registry.Exists(reg_val)) {
                var AssignmentSetOverride = $@"{reg_val}\Affinity Policy\AssignmentSetOverride";
                Tweak tweak = new Tweak { };
                tweak.name = "Set Affinity";
                tweak.description = $"Set Affinity";
                tweak.actions.Add(TweakAction.REGISTRY(AssignmentSetOverride, Registry.REG_DELETE, "3F", Microsoft.Win32.RegistryValueKind.Binary));
                return tweak;
            } else { return null; }
        }
    };
}
