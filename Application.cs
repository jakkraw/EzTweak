using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static EzTweak.Device_Tweak;

namespace EzTweak
{

    public partial class Application : Form
    {
        Dictionary<string, TabPage> tabss;

        Tab[] tabs_xml = null;

        public Application()
        {
            Log.WriteLine("EzTweak Started");
            InitializeComponent();
            Status.pipe += (msg) =>
            {
                status_loading.Text = msg;
            };
            Log.WriteLine($"User: {WindowsSystem.GetUserType()}");
            status_user.Text = $"👤 {WindowsSystem.GetUserType()}";
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Log.WriteLine("EzTweak Stopped");
        }

        private void App_Load(object sender, EventArgs e)
        {
            var xmlDocument = Parser.loadXML(Parser.xml_file);

            foreach (var item in Parser.LoadMenuItems(xmlDocument))
            {
                var context_items = CreateMenuItem(item);
                menu.Items.AddRange(context_items);
            }


            tabs_xml = Parser.LoadTweakTabs(xmlDocument);
            foreach (var tab in tabs_xml)
            {
                var tab_control = new TabPage
                {
                    AutoScroll = true,
                    ForeColor = SystemColors.ControlText,
                    Font = new Font("Arial", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                    Size = TweakControl.tab_size,
                    Text = tab.name
                };
                tab_control.Controls.Add(Label("Loading..."));
                tabs.Controls.Add(tab_control);
            }

            tabs.SelectedIndexChanged += on_tabs_changed;
            on_tabs_changed(null, null);
        }

        public static LinkLabel Label(string text)
        {
            var label = new LinkLabel();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Margin = new Padding(0, 0, 0, 0);
            label.Padding = new Padding(3, 3, 3, 3);
            label.LinkColor = Color.Black;
            label.AutoSize = true;
            return label;
        }

        private void on_tabs_changed(object sender, EventArgs e)
        {
            var selected_tab = tabs.SelectedTab;
            var tab = tabs_xml.Where(t => t.name == selected_tab.Text).First();
            if (tab == null)
            {
                return;
            }

            selected_tab.Controls.Clear();
            selected_tab.Controls.Add(Label("Loading..."));

            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();

            foreach (var section in tab.sections)
            {
                var section_panel = CreateFlyoutPanel();
                section_panel.SuspendLayout();
                section_panel.Controls.Add(TweakControl.Divider(section.name, ""));

                switch (section.type)
                {
                    case SectionType.SECTION:
                        {
                            var tweaks = section.tweaks.Select(t => new TweakControl(t, null)).ToArray();
                            var controls = tweaks.Select(t => t.panel).ToArray();
                            section_panel.Controls.AddRange(controls);
                        }
                        break;
                    case SectionType.IRQPRIORITY:
                        {
                            var container_tweaks = IRQ_Tweak.ALL_IRQ().Select(t => new TweakControl(t, null));
                            section_panel.Controls.AddRange(container_tweaks.Select(t => t.panel).ToArray());
                        }
                        break;
                    case SectionType.DEVICES:
                        {
                            var irqs = IRQ_Tweak.ALL_IRQ2();
                            var devices = Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());
                            section_panel.Controls.AddRange(devices.SelectMany(pair => pair.Value.Select(d => new TweakControl(DeviceTweaks(d, irqs.Where(i => i.description == d.Name).FirstOrDefault()), null)).Select(t => t.panel)).ToArray());
                        }
                        break;
                    case SectionType.APPX:
                        {
                            var container_tweaks = APPX_Tweak.ALL();
                            var tweak_controls = container_tweaks.Select(ts => new TweakControl(ts, null)).ToArray();
                            Array.ForEach(tweak_controls, tc => tc.run_button.Click += (a, b) => tc?.Hide());
                            var controls = tweak_controls.Select(t => t.panel).ToArray();
                            section_panel.Controls.AddRange(controls);
                        }
                        break;
                    case SectionType.SCHEDULED_TASKS:
                        {
                            var tasks = TaskTweak.GetTasks();
                            section_panel.Controls.AddRange(tasks.Select(t => new TweakControl(t, null).panel).ToArray());
                        }
                        break;
                    default: break;
                }
                section_panel.ResumeLayout();

                panel.Controls.Add(section_panel);
            }

            panel.ResumeLayout();
            selected_tab.Controls.Clear();
            selected_tab.Controls.Add(panel);
        }

    }
}
