using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EzTweak {
    public class Tweak {
        public string name;
        public string description;
        public System.Action turn_on;
        public System.Action turn_off;
        public Action<string> activate_value;
        public Func<Dictionary<string, string>> valid_values;
        public Func<string> current_value;
        public Func<string> status;
        public Func<bool> is_on;
        public Action on_click;
    }

    public class Container_Tweak : Tweak {
        public Tweak[] tweaks;

        public Container_Tweak(string name, string description, Tweak[] tweaks) {
            this.name = name;
            this.description = description;
            this.tweaks = tweaks;
            if (Array.TrueForAll(tweaks, t => t.turn_on != null)) {
                this.turn_on = () => Array.ForEach(tweaks, t => t.turn_on());
            }
            if (Array.TrueForAll(tweaks, t => t.turn_off != null)) {
                this.turn_off = () => Array.ForEach(tweaks, t => t.turn_off());
            }

            if (Array.TrueForAll(tweaks, t => t.activate_value != null)) {
                this.activate_value = (value) => Array.ForEach(tweaks, t => t.activate_value(value));
            }

            if (Array.TrueForAll(tweaks, t => t.is_on != null)) {
                this.is_on = () => tweaks.All(t => t.is_on());
            }

            if (Array.TrueForAll(tweaks, t => t.is_on != null)) {
                this.is_on = () => tweaks.All(t => t.is_on());
            }
        }
    }

    public class CMD_Tweak : Tweak {
        public CMD_Tweak(string name, string description, string on, string off, string status_command, string current_regex, string is_on_regex) {
            this.name = name;
            this.description = description;
            if (on != null) {
                this.turn_on = () => Start(on);
            }

            if (off != null) {
                this.turn_off = () => Start(off);
            }

            if (status_command != null) {
                this.status = () => $"[{current_value()}] CMD: {status_command}";

                if (current_regex != null) {
                    this.current_value = () => {
                        var output = Start(status_command, true);
                        var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
                        return match.Groups[1].Value;
                    };
                }

                if (is_on_regex != null) {
                    this.is_on = () => {
                        if (is_on_regex == null) { return false; }
                        var output = Start(status_command, true);
                        var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
                        return match.Success;
                    };
                }
            }

        }

        public static string Start(string command, bool quiet = false) {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = $"/C {command}";
            //cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.WaitForExit();
            var output = cmd.StandardOutput.ReadToEnd();
            if (!quiet) {
                Log.WriteLine($"{command}");
                Log.WriteLine($"{output}");
            }
            return output;
        }

        public static void Open(string[] cmd) {
            try {
                Process.Start(cmd[0], string.Join(" ", cmd.Skip(1).ToArray()));
            } catch (Exception e) {
                MessageBox.Show($"Error: {e.Message}", $"Failed to open {string.Join(" ", cmd)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class BCDEDIT_Tweak : Tweak {
        public BCDEDIT_Tweak(string name, string description, string property, string on_value, string off_value) {
            this.name = name;
            this.description = description;

            if (on_value != null) {
                this.turn_on = () => activate_value(on_value);
                this.is_on = () => Match(property, on_value);
            }

            if (off_value != null) {
                this.turn_off = () => activate_value(off_value);
            }

            this.activate_value = (string value) => {
                if (value == Registry.DELETE_TAG) {
                    Delete(property);
                } else {
                    Set(property, value);
                }
            };

            if (property != null) {
                this.current_value = () => Query(property);
                this.status = () => $"BCDEDIT: {current_value()}";
            }
        }

        public static string Query(string property) {
            IList<string> output = CMD_Tweak.Start("bcdedit /enum {current}", true).Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var regex = new Regex($@"^{property}\s([^\s\\].+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (string line in output) {
                var match = regex.Match(line);
                if (match.Success) {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        public static bool Match(string property, string value) {
            var a = Query(property);
            if (a == null) {
                return a == value;
            }
            if (value == null) { return false; }

            return a.ToLower() == value.ToLower();
        }

        public static void Set(string property, string value) {
            CMD_Tweak.Start($"bcdedit /set {property} {value}");
        }

        public static void Delete(string property) {
            CMD_Tweak.Start($"bcdedit /deletevalue {property}");
        }
    }

    public class Powershell_Tweak : Tweak {
        public Powershell_Tweak(string name, string description, string on_command, string off_command, string status_command, string current_regex, string is_on_regex) : this(name, description, on_command) {
            if (off_command != null) {
                this.turn_off = () => Start(off_command);
            }

            if (status_command != null) {
                if (current_regex != null) {
                    this.current_value = () => {
                        var output = Start(status_command, true);
                        var match = Regex.Match(output, current_regex, RegexOptions.Multiline);
                        return match.Groups[1].Value;
                    };

                    this.status = () => $"[{current_value()}] Powershell: {status_command}";
                }

                if (is_on_regex != null) {
                    this.is_on = () => {
                        var output = Start(status_command, true);
                        var match = Regex.Match(output, is_on_regex, RegexOptions.Multiline);
                        return match.Success;
                    };
                }
            }

        }

        public Powershell_Tweak(string name, string description, string on_command) {
            this.name = name;
            this.description = description;
            this.turn_on = () => Start(on_command);
        }

        public static string Start(string command, bool quiet = false) {
            var script = $"{command} | Out-String";
            PowerShell ps = PowerShell.Create();

            if (!quiet) {
                Log.WriteLine(command);
            }

            Collection<PSObject> results = ps.AddScript(script).Invoke();
            var output = string.Join(Environment.NewLine, results.Select(o => o.ToString()).ToList());
            return output;
        }
    }

    public class RegistryTweak : Tweak {
        public RegistryTweak(string name, string description, TweakType type, string path, string on_value, string off_value) : this(name, description, type, path) {
            if (on_value != null) {
                this.turn_on = () => activate_value(sanitize(on_value, type));
                this.is_on = () => current_value() == on_value;
            }

            if (off_value != null) {
                this.turn_off = () => activate_value(sanitize(off_value, type));
            }
        }

        public RegistryTweak(string name, string description, TweakType type, string path) {
            this.name = name;
            this.description = description;
            this.activate_value = (string value) => {
                Registry.Set(path, sanitize(value, type), (RegistryValueKind)type);
            };
            this.current_value = () =>
            sanitize(Registry.From(path, (RegistryValueKind)type), type);
            this.status = () => $"\"{path}\"={current_value()}";
        }

        public RegistryTweak(string name, string description, TweakType type, string path, Dictionary<string, string> values) : this(name, description, type, path) {
            this.valid_values = () => values;
        }

        protected static string sanitize(string value, TweakType type) {
            switch (type) {
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

    public class ServiceTweak : RegistryTweak {
        public ServiceTweak(string name, string description, string service, string on_value, string off_value) : base(name, description, TweakType.SERVICE, ServiceTweak.registry_path(service), on_value, off_value) {
            this.status += () => {
                var value = current_value();
                return $"{service} is {alias(value)}";
            };
        }

        private static string registry_path(string service) {
            return $@"HKLM\SYSTEM\CurrentControlSet\Services\{service}\Start";
        }

        private static string alias(string value) {
            if (sanitize("4", TweakType.DWORD) == value) {
                return "(Disabled)";
            }

            if (sanitize("3", TweakType.DWORD) == value) {
                return "(Manual)";
            }

            if (sanitize("2", TweakType.DWORD) == value) {
                return "(Automatic)";
            }
            return "";
        }
    }


    public class APPX_Tweak {
        public static List<Powershell_Tweak> ALL() {
            var output = Powershell_Tweak.Start("Get-AppxPackage | Select-Object -ExpandProperty Name", true);
            var lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var apps = lines.Where(x => x != "").OrderBy(x => x);
            return apps.Select(app => new Powershell_Tweak(
                    name: $"{app}",
                    description: "",
                    on_command: $"Get-AppxPackage *{app}* | Remove-AppxPackage"
                )).ToList();
        }
    }

    public class IRQ_Tweak {
        public class IRQ {
            public static Dictionary<string, SortedSet<ulong>> ReadDevices() {
                Dictionary<string, SortedSet<ulong>> devices = new Dictionary<string, SortedSet<ulong>>();
                foreach (ManagementObject Memory in new ManagementObjectSearcher(
                                "select * from Win32_DeviceMemoryAddress").Get()) {
                    // associate Memory addresses  with Pnp Devices
                    foreach (ManagementObject Pnp in new ManagementObjectSearcher(
                        "ASSOCIATORS OF {Win32_DeviceMemoryAddress.StartingAddress='" + Memory["StartingAddress"] + "'} WHERE RESULTCLASS  = Win32_PnPEntity").Get()) {
                        // associate Pnp Devices with IRQ
                        foreach (ManagementObject IRQ in new ManagementObjectSearcher(
                            "ASSOCIATORS OF {Win32_PnPEntity.DeviceID='" + Pnp["PNPDeviceID"] + "'} WHERE RESULTCLASS  = Win32_IRQResource").Get()) {
                            var key = Pnp["Caption"].ToString();
                            if (!devices.TryGetValue(key, out SortedSet<ulong> val)) {
                                val = new SortedSet<ulong>();
                                devices.Add(key, val);
                            }
                            var value = IRQ["IRQNumber"];
                            if (value != null) {
                                val.Add((UInt32)value);
                            }
                        }
                    }

                }
                return devices;
            }
        }

        public static List<RegistryTweak> ALL() {
            var values = new Dictionary<string, string> {
                { Registry.DELETE_TAG, Registry.DELETE_TAG },
                { "0x0", "0 (highest)" }, { "0x1", "1" }, { "0x2", "2" }, { "0x3", "3" }, { "0x4", "4" }, { "0x5", "5" },
                { "0x6", "6" }, { "0x7", "7" }, { "0x8", "8" }, { "0x9", "9" }, { "0x10", "10" }, { "0x11", "11" },
                { "0x12", "12" }, { "0x13", "13" }, { "0x14", "14" }, { "0x15", "15 (lowest)" }
            };

            var devices_dict = IRQ.ReadDevices().OrderBy(set => set.Value.FirstOrDefault());
            return devices_dict.Aggregate(
                new List<RegistryTweak> { },
                (acc, pair) => {
                    acc.AddRange(pair.Value.Select(irq => new RegistryTweak(
                        name: $"{pair.Key}",
                        description: "",
                        type: TweakType.DWORD,
                        path: $@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl\IRQ{irq}Priority",
                        values: values)));
                    return acc;
                }
               );
        }
    }


    public class Device_Tweak {
        public class Device {
            public static List<Device> All() {
                List<Device> devices = new List<Device>();
                using (var searcher = new ManagementObjectSearcher(
                    @"Select * From Win32_PNPEntity")) {
                    using (ManagementObjectCollection collection = searcher.Get()) {
                        foreach (var device in collection) {
                            var dev = new Device();
                            var keywords = new[] { "Name", "PNPDeviceID", "Status", "PNPClass", "Description", "Availability", "Caption", "ClassGuid", "ConfigManagerErrorCode", "ConfigManagerUserConfig", "CreationClassName", "DeviceID", "ErrorCleared", "ErrorDescription", "InstallDate", "LastErrorCode", "Manufacturer", "PowerManagementCapabilities", "PowerManagementSupported", "Present", "Service", "StatusInfo", "SystemCreationClassName", "SystemName" };
                            Array.ForEach(keywords, k => dev.values.Add(k, device.GetPropertyValue(k)?.ToString()));
                            devices.Add(dev);
                        }
                    }
                }
                return devices;
            }

            public object this[string a] {
                get { return values[a]; }
                set { values[a] = value.ToString(); }
            }

            public Dictionary<string, string> values = new Dictionary<string, string>();

            public string Name { get { return values.ContainsKey("Name") ? values["Name"] : null; } }
            public string PnpDeviceID { get { return values.ContainsKey("PNPDeviceID") ? values["PNPDeviceID"] : null; } }
            public string PNPClass { get { return values.ContainsKey("PNPClass") ? values["PNPClass"] : null; } }
            public string Status { get { return values.ContainsKey("Status") ? values["Status"] : null; } }
            public string Description { get { return values.ContainsKey("Description") ? values["Description"] : null; } }
            public string ClassGuid { get { return values.ContainsKey("ClassGuid") ? values["ClassGuid"] : null; } }
            public string FullInfo { get { return string.Join(Environment.NewLine, values.Select((p) => $"{p.Key}: {p.Value}")); } }
        }

        public static Tweak DisableDeviceTweak(Device device) {
            var name = "Disable Driver";
            var description = $"Disable {device.Name}{Environment.NewLine}{Environment.NewLine}{device.FullInfo}";
            var status_command = $"pnputil /enum-devices /instanceid \"{device.PnpDeviceID}\" | findstr /c:\"Status:\"";
            var status_regex = "Status:.*";
            var is_on_regex = "Status:.*Disabled";
            var on_command = $"pnputil /disable-device \"{device.PnpDeviceID}\"";
            var off_command = $"pnputil /enable-device \"{device.PnpDeviceID}\"";
            return new CMD_Tweak(name, description, on_command, off_command, status_command, status_regex, is_on_regex);
        }

        public static Tweak DeviceIdleRPIN(Device device) {
            var name = "Disable AllowIdleIrpInD3";
            var description = "Disable power saving option";
            var path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\AllowIdleIrpInD3";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Exists(path) ? tk : null;
        }

        public static Tweak EnhancedPowerManagementEnabled(Device device) {
            var name = "Disable Enhanced Power Management";
            var description = "Disable Enhanced Power Management";
            var path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\EnhancedPowerManagementEnabled";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Exists(path) ? tk : null;
        }

        public static Tweak MsiSupported(Device device) {
            var name = "Enable MSI";
            var description = "Enable MSI";
            var base_path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            var path = $@"{base_path}\MSISupported";
            var type = TweakType.DWORD;
            var off_value = "0";
            var on_value = "1";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Exists(base_path) ? tk : null;
        }

        public static Tweak DevicePriority(Device device) {
            var name = "Device Priority High";
            var description = "Device Priority High";
            var base_path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\DevicePriority";
            var type = TweakType.DWORD;
            var off_value = Registry.DELETE_TAG;
            var on_value = "3";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Exists(base_path) ? tk : null;
        }

        public static Tweak AssignmentSetOverride(Device device) {
            var name = "Set Affinity";
            var description = "Set Affinity";
            var base_path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\AssignmentSetOverride";
            var type = TweakType.DWORD;
            var off_value = Registry.DELETE_TAG;
            var on_value = "3F";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Exists(base_path) ? tk : null;
        }

        public static Tweak LinesLimitControl(Device device) {
            var name = "Message Number Limit";
            var description = "Set IRQ Limit";
            var base_path = $@"HKLM\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\MessageSignaledInterruptProperties\MessageNumberLimit";
            var type = TweakType.DWORD;
            var tk = new RegistryTweak(name, description, type, path);
            return Registry.Exists(base_path) ? tk : null;

        }
    }
}