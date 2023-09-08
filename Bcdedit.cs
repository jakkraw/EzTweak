using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EzTweak
{
    public static class Bcdedit
    {
        public static string Query(string property)
        {
            IList<string> output = Cmd.Start("bcdedit /enum {current}", true).Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var regex = new Regex($@"^{property}\s([^\s\\].+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (string line in output)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        public static bool Match(string property, string value)
        {
            var a = Query(property);
            if (a == null)
            {
                return a == value;
            }
            if (value == null) { return false; }

            return a.ToLower() == value.ToLower();
        }

        public static void Set(string property, string value)
        {
            Cmd.Start($"bcdedit /set {property} {value}");
        }

        public static void Delete(string property)
        {
            Cmd.Start($"bcdedit /deletevalue {property}");
        }
    }
}
