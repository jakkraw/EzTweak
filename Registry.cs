using CosmosKey.Utils;
using Hardware.Info;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace EzTweak {
    public static class Registry {
        public static string REG_DELETE = "[delete]";
        public static readonly string LocalMachineShort = @"HKLM";
        public static readonly string LocalMachine = @"HKEY_LOCAL_MACHINE";

        public static readonly string CurrentUserShort = @"HKCU";
        public static readonly string CurrentUser = @"HKEY_CURRENT_USER";

        public static bool GetBool(string path) {
            var value = Get(path);
            return !(value is "0" || value is null);
        }

        private static RegistryKey GetSubKey(string path, bool write = false, bool create = true) {
            string key_path = null;
            RegistryKey reg = null;

            if (path.StartsWith(LocalMachine)) {
                key_path = Path.GetDirectoryName(path.Remove(0, LocalMachine.Length + 1));
                reg = Microsoft.Win32.Registry.LocalMachine;
            }

            if (path.StartsWith(LocalMachineShort)) {
                key_path = Path.GetDirectoryName(path.Remove(0, LocalMachineShort.Length + 1));
                reg = Microsoft.Win32.Registry.LocalMachine;
            }

            if (path.StartsWith(CurrentUser)) {
                key_path = Path.GetDirectoryName(path.Remove(0, CurrentUser.Length + 1));
                reg = Microsoft.Win32.Registry.CurrentUser;
            }

            if (path.StartsWith(CurrentUserShort)) {
                key_path = Path.GetDirectoryName(path.Remove(0, CurrentUserShort.Length + 1));
                reg = Microsoft.Win32.Registry.CurrentUser;
            }

            var key = reg.OpenSubKey(key_path,write);
            if (key == null && create) {
                key = reg.CreateSubKey(key_path);
            }
            return key;
        }

        public static string Get(string path, RegistryValueKind type = RegistryValueKind.DWord) {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path)) {
                if (key == null) return null;
                if (type == RegistryValueKind.Binary) {
                    byte[] o = (byte[])key.GetValue(key_name);
                    if (o == null) return null;
                    return String.Join(",", o.Select(a => a.ToString("X").PadLeft(2, '0')));
                }
                if (type == RegistryValueKind.DWord)
                {
                    Object o = key.GetValue(key_name);
                    if (o == null) return null;
                    return $"0x{((Int32)o).ToString("X")}";
                }
                else {
                    Object o = key.GetValue(key_name);
                    if (o == null) return null;
                    return o.ToString();
                }
            }
        }

        public static bool Exists(string path) {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, false, false)) {
                return key != null;
            }
        }


        public static void Set(string path, string value, RegistryValueKind type = RegistryValueKind.DWord) {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, true)) {

                if (type == RegistryValueKind.Binary) {
                    var data = value.Split(',').Select(x => Convert.ToByte(x, 16))
    .ToArray();
                    key.SetValue(key_name, data, type);
                }
                if (type == RegistryValueKind.DWord)
                {
                    int n;
                    if (value.StartsWith("0x"))
                    {
                        n = Int32.Parse(value.Substring(2), NumberStyles.HexNumber);
                    }
                    else
                    {
                        n = Int32.Parse(value);
                    }
                    key.SetValue(key_name, n, type);
                }
                else {
                    key.SetValue(key_name, value, type);
                }

                Log.WriteLine($"{path}=\"{value}\"");
            }
        }

        public static void Delete(string path) {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, true)) {
                key.DeleteValue(key_name, false);
                Log.WriteLine($"{path} 🗑");
            }
        }

        public static void Set(string path, uint value) {
            Set(path, value.ToString());
        }

        public static void Set(string path, bool value) {
            Set(path, value ? "1" : "0");
        }


        public static string AsHex(string value)
        {
            if (value == null)
            {
                return null;
            }

            if(value == Registry.REG_DELETE)
            {
                return value;
            }

            int n;
            if (value.StartsWith("0x"))
            {
                n = Int32.Parse(value.Substring(2), NumberStyles.HexNumber);
            }
            else
            {
                n = Int32.Parse(value);
            }

            return $"0x{n.ToString("X")}";
        }
    }
}
