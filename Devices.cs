using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace EzTweak
{

    public class IRQ
    {
        public static Dictionary<string, SortedSet<ulong>> ReadDevices()
        {
            Dictionary<string, SortedSet<ulong>> devices = new Dictionary<string, SortedSet<ulong>>();
            foreach (ManagementObject Memory in new ManagementObjectSearcher(
                            "select * from Win32_DeviceMemoryAddress").Get())
            {
                // associate Memory addresses  with Pnp Devices
                foreach (ManagementObject Pnp in new ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DeviceMemoryAddress.StartingAddress='" + Memory["StartingAddress"] + "'} WHERE RESULTCLASS  = Win32_PnPEntity").Get())
                {
                    // associate Pnp Devices with IRQ
                    foreach (ManagementObject IRQ in new ManagementObjectSearcher(
                        "ASSOCIATORS OF {Win32_PnPEntity.DeviceID='" + Pnp["PNPDeviceID"] + "'} WHERE RESULTCLASS  = Win32_IRQResource").Get())
                    {
                        var key = Pnp["Caption"].ToString();
                        if (!devices.TryGetValue(key, out SortedSet<ulong> val))
                        {
                            val = new SortedSet<ulong>();
                            devices.Add(key, val);
                        }
                        var value = IRQ["IRQNumber"];
                        if (value != null)
                        {
                            val.Add((UInt32)value);
                        }
                    }
                }

            }
            return devices;
        }
    }


    public class USB
    {
        public static List<USB> ReadDevices()
        {
            List<USB> devices = new List<USB>();

            using (var searcher = new ManagementObjectSearcher(
                @"Select * From Win32_USBHub"))
            {
                using (ManagementObjectCollection collection = searcher.Get())
                {

                    foreach (var device in collection)
                    {
                        devices.Add(new USB(
                            (string)device.GetPropertyValue("DeviceID"),
                            (string)device.GetPropertyValue("Description").ToString(),
                            (string)device.GetPropertyValue("Caption")
                            ));
                    }
                }
            }

            return devices;
        }

        public USB(string deviceID, string pnpDeviceID, string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
    }


    public class Device
    {
        public static List<Device> All()
        {
            List<Device> devices = new List<Device>();

            using (var searcher = new ManagementObjectSearcher(
                @"Select * From Win32_PNPEntity"))
            {
                using (ManagementObjectCollection collection = searcher.Get())
                {

                    foreach (var device in collection)
                    {
                        var dev = new Device();
                        var keywords = new[] { "Name", "PNPDeviceID", "Status", "PNPClass", "Description", "Availability", "Caption", "ClassGuid", "ConfigManagerErrorCode", "ConfigManagerUserConfig", "CreationClassName", "DeviceID", "ErrorCleared", "ErrorDescription", "InstallDate", "LastErrorCode", "Manufacturer", "PowerManagementCapabilities", "PowerManagementSupported", "Present", "Service", "StatusInfo", "SystemCreationClassName", "SystemName" };
                        foreach (var keyword in keywords)
                        {
                            var val = device.GetPropertyValue(keyword);
                            dev.values.Add(keyword, val != null ? val.ToString() : null);
                        }

                        devices.Add(dev);
                    }
                }
            }

            return devices;
        }

        public object this[string a]
        {
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


        public static Tk DisableDeviceTweak(Device device)
        {
            var name = "Disable Driver";
            var description = $"Disable {device.Name}{Environment.NewLine}{Environment.NewLine}{device.FullInfo}";
            var status_command = $"pnputil /enum-devices /instanceid \"{device.PnpDeviceID}\" | findstr /c:\"Status:\"";
            var status_regex = "Status:.*";
            var is_on_regex = "Status:.*Disabled";
            var on_command = $"pnputil /disable-device \"{device.PnpDeviceID}\"";
            var off_command = $"pnputil /enable-device \"{device.PnpDeviceID}\"";
            return new CMD_Tweak(name, description, on_command, off_command, status_command, status_regex, is_on_regex);
        }

        public static Tk DeviceIdleRPIN(Device device)
        {
            var name = "Disable AllowIdleIrpInD3";
            var description = "Disable power saving option";
            var path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\AllowIdleIrpInD3";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Get(path) == null ? null : tk;
        }

        public static Tk EnhancedPowerManagementEnabled(Device device)
        {
            var name = "Disable Enhanced Power Management";
            var description = "Disable Enhanced Power Management";
            var path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\EnhancedPowerManagementEnabled";
            var type = TweakType.DWORD;
            var off_value = "1";
            var on_value = "0";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Get(path) == null ? null : tk;
        }

        public static Tk MsiSupported(Device device)
        {
            var name = "Enable MSI";
            var description = "Enable MSI";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties";
            var path = $@"{base_path}\MSISupported";
            var type = TweakType.DWORD;
            var off_value = "0";
            var on_value = "1";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Get(base_path) == null ? null : tk;
        }

        public static Tk DevicePriority(Device device)
        {
            var name = "Device Priority High";
            var description = "Device Priority High";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\DevicePriority";
            var type = TweakType.DWORD;
            var off_value = Registry.REG_DELETE;
            var on_value = "3";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Get(base_path) == null ? null : tk;
        }

        public static Tk AssignmentSetOverride(Device device)
        {
            var name = "Set Affinity";
            var description = "Set Affinity";
            var base_path = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters\Interrupt Management";
            var path = $@"{base_path}\Affinity Policy\AssignmentSetOverride";
            var type = TweakType.DWORD;
            var off_value = Registry.REG_DELETE;
            var on_value = "3F";
            var tk = new RegistryTweak(name, description, type, path, on_value, off_value);
            return Registry.Get(base_path) == null ? null : tk;
        }
    }
}