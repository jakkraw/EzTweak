using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EzTweak {
    public enum ActionType {
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
    }

    public enum SectionType {
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

    public class XmlAction {
        [XmlAttribute]
        public ActionType type { get; set; }
        [XmlElement("path")]
        public List<string> paths { get; set; }
        public string on { get; set; }
        public string off { get; set; }

        public string lookup_regex { get; set; }
        public string on_regex { get; set; }
        public string lookup { get; set; }

        public string property { get; set; }

        [XmlElement("service")]
        public List<string> services { get; set; }

        public string backup { get; set; } = null;
    }
    public class XmlTweak {
        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("action")]
        public List<XmlAction> actions { get; set; }

        public string description { get; set; }
    }

    public class XmlSection {
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public SectionType type { get; set; } = SectionType.TWEAKS;

        [XmlElement("tweak")]
        public List<XmlTweak> tweaks { get; set; }
    }

    public class XmlTab {
        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("section")]
        public List<XmlSection> sections { get; set; } = new List<XmlSection> { };
    }

    [XmlRoot("EzTweak")]
    public class XmlDoc {
        [XmlElement("tab")]
        public List<XmlTab> tabs { get; set; } = new List<XmlTab> { };
    }

    public class Tab {
        public string name;
        public List<Section> sections = new List<Section>();
    }

    public class Section {
        public string name;
        public SectionType type { get; set; }
        public List<Tweak> tweaks = new List<Tweak>();
    }

    public class Parser {

        public static XmlDoc loadXML(string filename) {
            using (StreamReader reader = new StreamReader(filename)) {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDoc));
                return (XmlDoc)serializer.Deserialize(reader);
            }
        }
        public static List<Tab> LoadTabs(XmlDoc xmlDocument) {
            List<Tab> tabs = new List<Tab> { };
            foreach (var xml_tab in xmlDocument.tabs) {
                var tab = new Tab {
                    name = xml_tab.name,
                };
                foreach (var xml_sections in xml_tab.sections) {
                    var section = new Section {
                        name = xml_sections.name,
                        type = xml_sections.type
                    };

                    foreach (var xml_tweak in xml_sections.tweaks) {
                        Tweak tweak = new Tweak { };
                        tweak.name = xml_tweak.name;
                        tweak.description = xml_tweak.description;

                        foreach (var xml_action in xml_tweak.actions) {
                            ActionType type = xml_action.type;
                            switch (type) {
                                case ActionType.DWORD:
                                case ActionType.REG_SZ:
                                case ActionType.BINARY:
                                    foreach (var path in xml_action.paths)
                                        tweak.actions.Add(TweakAction.REGISTRY(path, xml_action.off, xml_action.on, (RegistryValueKind)type));
                                    break;

                                case ActionType.CMD:
                                    tweak.actions.Add(TweakAction.CMD(xml_action.off, xml_action.on, xml_action.lookup, xml_action.lookup_regex, xml_action.on_regex));
                                    break;
                                case ActionType.POWERSHELL:
                                    tweak.actions.Add(TweakAction.POWERSHELL(xml_action.off, xml_action.on, xml_action.lookup, xml_action.lookup_regex, xml_action.on_regex));
                                    break;
                                case ActionType.BCDEDIT:
                                    tweak.actions.Add(TweakAction.BCDEDIT(xml_action.property, xml_action.off, xml_action.on));
                                    break;
                                case ActionType.SERVICE:
                                    foreach (var service in xml_action.services)
                                        tweak.actions.Add(TweakAction.SERVICE(service, xml_action.off, xml_action.on));
                                    break;
                            }
                        }
                        section.tweaks.Add(tweak);

                    }
                    tab.sections.Add(section);
                }

                tabs.Add(tab);
            }

            return tabs;
        }

    }
}
