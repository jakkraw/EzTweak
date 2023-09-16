using System;
using System.Collections.Generic;

namespace EzTweak
{
    public class BCDEDIT_Tweak : Tk
    {
        string property;
        string value_on;
        string value_off;

        public BCDEDIT_Tweak(string name, string description, string property, string on_value, string off_value) : base(name,description,ActionType.BCDEDIT)
        {
            this.property = property;
            this.value_on = on_value;
            this.value_off = off_value;
        }

        public override void turn_off()
        {
            activate_value(value_off);
        }

        public override void turn_on()
        {
            activate_value(value_on);
        }

        public override void activate_value(string value)
        {
            if (value == Registry.REG_DELETE)
            {
                Bcdedit.Delete(property);
            }
            else
            {
                Bcdedit.Set(property, value);
            }
        }

        public override string status()
        {
            return $"BCDEDIT: {current_value()}";
        }

        public override string current_value()
        {
            return Bcdedit.Query(property);
        }

        public override bool is_on()
        {
            return Bcdedit.Match(property, value_on);
        }

        public override List<string> valid_values()
        {
            throw new NotImplementedException();
        }
    }
}
