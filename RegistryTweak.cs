using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace EzTweak
{
    public class RegistryTweak : Tk
    {
        protected string path;
        string on_value;
        string off_value;

        public RegistryTweak(string name, string description, TweakType type, string path, string on_value, string off_value) : base(name, description, type)
        {
            this.path = path;
            this.on_value = sanitize(on_value);
            this.off_value = sanitize(off_value);
        }

        protected string sanitize(string value)
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

        public override void turn_off()
        {
            activate_value(off_value);
        }

        public override void turn_on()
        {
            activate_value(on_value);
        }

        public override void activate_value(string value)
        {
            Registry.Set(path, sanitize(value), (RegistryValueKind)type);
        }

        public override string status()
        {
            return $"\"{path}\"={current_value()}";
        }

        public override string current_value()
        {
            return sanitize(Registry.From(path, (RegistryValueKind)type));
        }

        public override bool is_on()
        {
            return current_value() == on_value;
        }

        public override List<string> valid_values()
        {
            return new List<string> { on_value, off_value };
        }
    }
}
