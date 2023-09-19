using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EzTweak
{
    public static class Registry
    {
        public static string DELETE_TAG = "🗑";
        public static readonly string LocalMachineShort = @"HKLM";
        public static readonly string LocalMachine = @"HKEY_LOCAL_MACHINE";

        public static readonly string CurrentUserShort = @"HKCU";
        public static readonly string CurrentUser = @"HKEY_CURRENT_USER";

        private static RegistryKey GetSubKey(string path, bool write = false, bool create = true)
        {
            string key_path = null;
            RegistryKey reg = null;

            if (path.StartsWith(LocalMachine))
            {
                key_path = Path.GetDirectoryName(path.Remove(0, LocalMachine.Length + 1));
                reg = Microsoft.Win32.Registry.LocalMachine;
            }

            if (path.StartsWith(LocalMachineShort))
            {
                key_path = Path.GetDirectoryName(path.Remove(0, LocalMachineShort.Length + 1));
                reg = Microsoft.Win32.Registry.LocalMachine;
            }

            if (path.StartsWith(CurrentUser))
            {
                key_path = Path.GetDirectoryName(path.Remove(0, CurrentUser.Length + 1));
                reg = Microsoft.Win32.Registry.CurrentUser;
            }

            if (path.StartsWith(CurrentUserShort))
            {
                key_path = Path.GetDirectoryName(path.Remove(0, CurrentUserShort.Length + 1));
                reg = Microsoft.Win32.Registry.CurrentUser;
            }

            var key = reg.OpenSubKey(key_path, write);
            if (key == null && create)
            {
                key = reg.CreateSubKey(key_path);
            }
            return key;
        }


        public static void Set(string path, string value, RegistryValueKind type)
        {
            if (type == RegistryValueKind.String)
            {
                Set_REG_SZ(path, To_REG_SZ(value));
            }

            if (type == RegistryValueKind.DWord)
            {
                Set_DWORD(path, To_DWORD(value));
            }

            if (type == RegistryValueKind.Binary)
            {
                Set_BINARY(path, To_BINARY(value));
            }
        }

        public static void Set(string path, object value, RegistryValueKind type)
        {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, true))
            {
                if (value != null)
                {
                    key.SetValue(key_name, value, type);
                    Log.WriteLine($"{path} {type} {value}");
                }
                else
                {
                    key.DeleteValue(key_name, false);
                    Log.WriteLine($"{path} {Registry.DELETE_TAG}");
                }
            }
        }

        public static object Get(string path)
        {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, false, false))
            {
                return key?.GetValue(key_name);
            }
        }

        public static string From(string path, RegistryValueKind type)
        {
            switch (type)
            {
                case RegistryValueKind.DWord:
                    return From_DWORD(Get_DWORD(path));
                case RegistryValueKind.Binary:
                    return From_BINARY(Get_BINARY(path));
                case RegistryValueKind.String:
                    return From_REG_SZ(Get_REG_SZ(path));
                default: return null;
            }
        }

        public static string From_REG_SZ(string value)
        {
            return value ?? Registry.DELETE_TAG;
        }

        public static string From_BINARY(byte[] value)
        {
            var v = value?.Select(a => a.ToString("X").PadLeft(2, '0'));
            return v != null ? string.Join(",", v) : Registry.DELETE_TAG;
        }

        public static string From_DWORD(UInt32? v)
        {
            return v != null && v.HasValue ? $"0x{v.Value:X}" : Registry.DELETE_TAG;
        }

        public static UInt32? To_DWORD(string v)
        {
            if (v == null || v == Registry.DELETE_TAG)
            {
                return null;
            }

            return v.StartsWith("0x") ? UInt32.Parse(v.Substring(2), NumberStyles.HexNumber) : UInt32.Parse(v);
        }

        public static string To_REG_SZ(string v)
        {
            if (v == null || v == Registry.DELETE_TAG)
            {
                return null;
            }

            return v;
        }

        public static byte[] To_BINARY(string v)
        {
            if (v == null || v == Registry.DELETE_TAG || v == "")
            {
                return null;
            }
            return v.Split(',').Select(x => Convert.ToByte(x, 16)).ToArray();
        }

        public static UInt32? Get_DWORD(string path)
        {
            var v = Get(path);
            if (v == null) return null;
            return (UInt32?)Convert.ToInt32(v.ToString());
        }

        public static void Set_DWORD(string path, UInt32? value)
        {
            Set(path, value, RegistryValueKind.DWord);
        }

        public static byte[] Get_BINARY(string path)
        {
            return Get(path) as byte[];
        }

        public static void Set_BINARY(string path, byte[] value)
        {
            Set(path, value, RegistryValueKind.Binary);
        }

        public static string Get_REG_SZ(string path)
        {
            return Get(path) as string;
        }

        public static void Set_REG_SZ(string path, string value)
        {
            Set(path, (object)value, RegistryValueKind.String);
        }

        public static bool Exists(string path)
        {
            using (RegistryKey key = GetSubKey(path, false, false))
            {
                return key != null;
            }
        }

        public static void Delete(string path)
        {
            string key_name = Path.GetFileName(path);
            using (RegistryKey key = GetSubKey(path, true, false))
            {
                key.DeleteValue(key_name, false);
                Log.WriteLine($"{path} {Registry.DELETE_TAG}");
            }
        }
    }
}
