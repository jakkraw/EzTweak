using System;
using System.Collections.Generic;
using System.Linq;

namespace EzTweak
{
    public static class APPX
    {

        public static List<string> ALL()
        {
            var output = Powershell.Start("Get-AppxPackage | Select-Object -ExpandProperty Name", true);
            var lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return lines.Where(x => x != "").OrderBy(x => x).ToList();
        }
    }
}
