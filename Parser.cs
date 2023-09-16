using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EzTweak
{
    public enum ActionType
    {
        [XmlEnum("DWORD")]
        DWORD = RegistryValueKind.DWord,
        [XmlEnum("REG_SZ")]
        REG_SZ = RegistryValueKind.String,
        [XmlEnum("BINARY")]
        BINARY = RegistryValueKind.Binary,
        [XmlEnum("CMD")]
        CMD = 100,
        [XmlEnum("POWERSHELL")]
        POWERSHELL,
        [XmlEnum("BCDEDIT")]
        BCDEDIT,
        [XmlEnum("SERVICE")]
        SERVICE,
        [XmlEnum("CONTAINER")]
        CONTAINER,
    }

    public enum SectionType
    {
        [XmlEnum("TWEAKS")]
        TWEAKS,
        [XmlEnum("IRQPRIORITY")]
        IRQPRIORITY,
        [XmlEnum("APPXDELETE")]
        APPXDELETE,
        [XmlEnum("DEVICES")]
        DEVICES,
        [XmlEnum("SCHEDULED_TASKS")]
        SCHEDULED_TASKS,
        [XmlEnum("APPX")]
        APPX
    }

    public class XmlTweak
    {
        [XmlAttribute]
        public ActionType type { get; set; } = ActionType.CONTAINER;

        [XmlAttribute]
        public string name { get; set; } = "";

        public string on { get; set; }
        public string off { get; set; }

        public string lookup_regex { get; set; }
        public string on_regex { get; set; }
        public string lookup { get; set; }

        public string property { get; set; }

        public string description { get; set; } = "";

        [XmlElement("path")]
        public List<string> paths { get; set; } = new List<string> { };

        [XmlElement("service")]
        public List<string> services { get; set; } = new List<string> { };

        [XmlElement("tweak")]
        public List<XmlTweak> tweaks { get; set; } = new List<XmlTweak> { };


        public Tk parse()
        {
            if (type == ActionType.CONTAINER)
            {
                var tks = tweaks.Select(t => t.parse()).ToList<Tk>();
                return tks.Count > 1 ? new Container_Tweak(name, description, tks) : tks.First();
            }

            if (type == ActionType.SERVICE)
            {
                var tks = services.Select(service => new ServiceTweak(name, description, service, on, off)).ToList<Tk>();
                return tks.Count > 1 ? new Container_Tweak(name, description, tks) : tks.First();
            }

            if (new[] { ActionType.DWORD, ActionType.REG_SZ, ActionType.BINARY }.Contains(type))
            {
                var tks = paths.Select(path => new RegistryTweak(name, description, type, path, on, off)).ToList<Tk>();
                return tks.Count > 1 ? new Container_Tweak(name, description, tks) : tks.First();
            }

            if (type == ActionType.BCDEDIT)
            {
                return new BCDEDIT_Tweak(name, description,property,on,off);
            }

            if (type == ActionType.POWERSHELL)
            {
                return new Powershell_Tweak(name, description, on, off,lookup, lookup_regex, on_regex);
            }

            if (type == ActionType.CMD)
            {
                return new CMD_Tweak(name, description, on, off, lookup, lookup_regex, on_regex);
            }

            return null;
        }
    }

    public class XmlSection
    {
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public SectionType type { get; set; } = SectionType.TWEAKS;

        [XmlElement("tweak")]
        public List<XmlTweak> tweaks { get; set; } = new List<XmlTweak> { };
    }

    public class XmlTab
    {
        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("section")]
        public List<XmlSection> sections { get; set; } = new List<XmlSection> { };
    }

    [XmlRoot("EzTweak")]
    public class XmlDoc
    {
        [XmlElement("tab")]
        public List<XmlTab> tabs { get; set; } = new List<XmlTab> { };
    }

    public class Tab
    {
        public string name;
        public List<Section> sections = new List<Section>();

        public Tab(XmlTab xml)
        {
            name = xml.name;
            sections = xml.sections.Select(x => new Section(x)).ToList();
        }
    }

    public class Section
    {
        public string name;
        public SectionType type { get; set; }
        public List<Tk> tweaks = new List<Tk>();

        public Section(XmlSection xml)
        {
            name = xml.name;
            type = xml.type;

            tweaks = xml.tweaks.Select(t => t.parse()).ToList();
        }
    }

    public class Parser
    {

        public static XmlDoc loadXML(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDoc));
                return (XmlDoc)serializer.Deserialize(reader);
            }
        }

        static Tweak[] CreateAction(XmlTweak a)
        {
            switch (a.type)
            {
                case ActionType.DWORD:
                    return a.paths.Select(path => Tweak.REGISTRY_DWORD(path, a.off, a.on)).Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray();
                case ActionType.REG_SZ:
                    return a.paths.Select(path => Tweak.REGISTRY_REG_SZ(path, a.off, a.on)).Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray();
                case ActionType.BINARY:
                    return a.paths.Select(path => Tweak.REGISTRY_BINARY(path, a.off, a.on)).Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray();
                case ActionType.SERVICE:
                    return a.services.Select(service => Tweak.SERVICE(service, a.off, a.on)).Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray();
                case ActionType.CMD:
                    return new[] { Tweak.CMD(a.off, a.on, a.lookup, a.lookup_regex, a.on_regex) }.Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray();
                case ActionType.POWERSHELL:
                    return new[] { Tweak.POWERSHELL(a.off, a.on, a.lookup, a.lookup_regex, a.on_regex) }.Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray(); ;
                case ActionType.BCDEDIT:
                    return new[] { Tweak.BCDEDIT(a.property, a.off, a.on) }.Select(t => { t.name = a.name; t.description = a.description; return t; }).ToArray(); ;
                case ActionType.CONTAINER:
                    return null; 
                default: return null;
            }
        }

        public static List<Tab> LoadTabs(XmlDoc xmlDocument)
        {
            return xmlDocument.tabs.Select(xml => new Tab(xml)).ToList();
        }
    }

}
