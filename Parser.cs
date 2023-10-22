using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EzTweak {
    public enum TweakType {
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
        [XmlEnum("TWEAKS")]
        TWEAKS
    }

    public enum SectionType {
        [XmlEnum("SECTION")]
        SECTION,
        [XmlEnum("IRQPRIORITY")]
        IRQPRIORITY,
        [XmlEnum("APPX")]
        APPX,
        [XmlEnum("DEVICES")]
        DEVICES,
        [XmlEnum("SCHEDULED_TASKS")]
        SCHEDULED_TASKS
    }

    public class XmlTweak {
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

        [XmlChoiceIdentifier("tweakTypes")]
        [XmlElement("DWORD")]
        [XmlElement("REG_SZ")]
        [XmlElement("BINARY")]
        [XmlElement("CMD")]
        [XmlElement("POWERSHELL")]
        [XmlElement("BCDEDIT")]
        [XmlElement("SERVICE")]
        [XmlElement("TWEAKS")]
        public XmlTweak[] tweaks { get; set; }

        [XmlIgnore]
        public TweakType[] tweakTypes { get; set; }

        public Tweak parse(TweakType type) {
            switch (type) {
                case TweakType.TWEAKS: {
                        var tks = tweaks?.Zip(tweakTypes, (tweak, tweak_type) => tweak.parse(tweak_type)).ToArray() ?? new Tweak[] { };
                        return new Container_Tweak(name, description, tks);
                    }
                case TweakType.SERVICE: {
                        var tks = services?.Select(service => new ServiceTweak(service, on, off)).ToArray<Tweak>() ?? new Tweak[] { };
                        return new Container_Tweak(name, description, tks);
                    }
                case TweakType.DWORD:
                case TweakType.REG_SZ:
                case TweakType.BINARY: {
                        var tks = paths?.Select(path => new RegistryTweak(type, path, on, off)).ToArray() ?? new Tweak[] { };
                        return new Container_Tweak(name, description, tks);
                    }
                case TweakType.BCDEDIT:
                    {
                        var tks = new Tweak[] { new BCDEDIT_Tweak(property, on, off) };
                        return new Container_Tweak(name, description, tks);
                    }
                case TweakType.POWERSHELL:
                    {
                        var tks = new Tweak[] { new Powershell_Tweak(on, off, lookup, lookup_regex, on_regex) };
                        return new Container_Tweak(name, description, tks);
                    }
                case TweakType.CMD:
                    {
                        var tks = new Tweak[] { new CMD_Tweak(on, off, lookup, lookup_regex, on_regex) };
                        return new Container_Tweak(name, description, tks);
                    }
                default: throw new Exception($"Unknown TweakType {type}");
            }
        }


        public Tweak[] parse2(TweakType type)
        {
            switch (type)
            {
                case TweakType.TWEAKS:
                    {
                        var a = tweaks?.Zip(tweakTypes, (tweak, tweak_type) => tweak.parse2(tweak_type)).ToArray() ?? new Tweak[][] { };
                        var b = a.SelectMany(x => x).ToArray();
                        return b;
                    }
                case TweakType.SERVICE:
                    {
                        return services?.Select(service => new ServiceTweak(service, on, off)).ToArray<Tweak>() ?? new Tweak[] { };
                    }
                case TweakType.DWORD:
                case TweakType.REG_SZ:
                case TweakType.BINARY:
                    {
                        return paths?.Select(path => new RegistryTweak(type, path, on, off)).ToArray() ?? new Tweak[] { };
                    }
                case TweakType.BCDEDIT:
                    {
                        return new Tweak[] { new BCDEDIT_Tweak(property, on, off) };
                    }
                case TweakType.POWERSHELL:
                    {
                        return new Tweak[] { new Powershell_Tweak(on, off, lookup, lookup_regex, on_regex) };
                    }
                case TweakType.CMD:
                    {
                        return new Tweak[] { new CMD_Tweak(on, off, lookup, lookup_regex, on_regex) };
                    }
                default: throw new Exception($"Unknown TweakType {type}");
            }
        }

    }

    public class XmlSection {
        [XmlAttribute]
        public string name { get; set; }

        [XmlChoiceIdentifier("tweakTypes")]
        [XmlElement("DWORD")]
        [XmlElement("REG_SZ")]
        [XmlElement("BINARY")]
        [XmlElement("CMD")]
        [XmlElement("POWERSHELL")]
        [XmlElement("BCDEDIT")]
        [XmlElement("SERVICE")]
        [XmlElement("TWEAKS")]
        public XmlTweak[] tweaks { get; set; }

        [XmlIgnore]
        public TweakType[] tweakTypes { get; set; }
    }

    public class XmlTab {
        [XmlAttribute]
        public string name { get; set; }

        [XmlIgnore]
        public SectionType[] sectionTypes;

        [XmlChoiceIdentifier("sectionTypes")]
        [XmlElement("IRQPRIORITY")]
        [XmlElement("APPX")]
        [XmlElement("DEVICES")]
        [XmlElement("SCHEDULED_TASKS")]
        [XmlElement("SECTION")]
        public XmlSection[] sections { get; set; }
    }

    [XmlRoot("EZTWEAK")]
    public class XmlDoc {
        [XmlElement("TAB")]
        public XmlTab[] tabs { get; set; }
    }

    public class Tab {
        public string name;
        public Section[] sections;

        public Tab(XmlTab xml) {
            name = xml.name;
            sections = xml.sections?.Zip(xml.sectionTypes, (x, st) => new Section(x, st)).ToArray() ?? new Section[] { };
        }
    }

    public class Section {
        public string name;
        public SectionType type { get; set; }
        public Tweak[] tweaks;

        public Section(XmlSection xml, SectionType stype) {
            name = xml.name;
            type = stype;
            tweaks = xml.tweaks?.Zip(xml.tweakTypes, (t, at) => new Container_Tweak(t.name, t.description, t.parse2(at))).ToArray() ?? new Tweak[] { };
        }
    }

    public class Parser {
        public static string xml_file = "tweaks.xml";
        public static XmlDoc loadXML(string filename) {
            using (StreamReader reader = new StreamReader(filename)) {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDoc));
                return (XmlDoc)serializer.Deserialize(reader);
            }
        }

        public static List<Tab> LoadTabs(XmlDoc xmlDocument) {
            return xmlDocument.tabs.Select(xml => new Tab(xml)).ToList();
        }
    }

}
