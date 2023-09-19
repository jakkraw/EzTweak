using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EzTweak {
    public partial class App : Form {
        public App() {
            Log.WriteLine("EzTweak Started");
            InitializeComponent();

            Func<KeyValuePair<string, string[]>, ToolStripMenuItem> to_tool_menu = (pair) => {
                var control = new ToolStripMenuItem {
                    Size = new Size(180, 22),
                    Text = pair.Key,
                };
                control.Click += (a, b) => CMD_Tweak.Open(pair.Value);
                return control;
            };

            menu_open.DropDownItems.AddRange(new Dictionary<string, string[]> {
                { "Logs", new [] { Log.log_file } },
                { "Tweaks Schema", new [] { $"notepad", Parser.xml_file } }
            }.Select(to_tool_menu).ToArray());

            menu_open.DropDownItems.Add(new ToolStripSeparator() { Size = new Size(177, 6) });

            menu_open.DropDownItems.AddRange(new Dictionary<string, string[]> {
                { "Registry Editor", new[] { "regedit" } },
                { "Startup Programs", new[]  { "taskmgr", "/0", "/startup" } },
                { "Windows Features", new[] { "optionalfeatures" } },
                { "Add/Remove Programs", new[] { "appwiz.cpl" } },
                { "Task Scheduler", new[] { "taskschd.msc" } },
                { "Group Policy", new[] { "gpedit.msc" } },
                { "System Configuration", new[] { "msconfig" } },
                { "Restore Points", new[] { "rstrui.exe" } },
                { "Internet Options", new[] { "inetcpl.cpl" } },
                { "Event Log", new[] { "eventvwr" } },
                { "Sounds", new[] { "control", "mmsys.cpl", "sounds" } },
                { "Network Connections", new[] { "ncpa.cpl" } },
                { "Windows Updates", new[] { "ms-settings:windowsupdate" } },
                { "Settings", new[] { "ms-settings:" } },
                { "Device Manager", new[] { "devmgmt.msc" } },
                { "Devices and Printers", new[] { "control printers" } },
                { "Appearance and performance", new[] { "SystemPropertiesPerformance.exe" } },
                { "Cmd", new[] { "cmd" } },
                { "PowerShell", new[] { "powershell" } },
            }.Select(to_tool_menu).ToArray());
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);
            Log.WriteLine("EzTweak Stopped");
        }

        private void App_Load(object sender, EventArgs e) {
            var tweaks_xml = Parser.xml_file;
            var xmlDocument = Parser.loadXML(tweaks_xml);
            var tabs = Parser.LoadTabs(xmlDocument);
            foreach (var tab in tabs) {
                var tab_control = CreateTab(tab);
                this.tabs.Controls.Add(tab_control);
            }
        }
    }
}
