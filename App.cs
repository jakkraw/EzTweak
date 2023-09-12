using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;


namespace EzTweak
{
    public partial class App : Form
    {
        public App()
        {
            Log.WriteLine("EzTweak Started");
            InitializeComponent();
            Interface.info_box = description_textbox;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Log.WriteLine("EzTweak Stopped");
        }

        private TabPage CreateTab(Tab tab)
        {
            var tab_control = new TabPage
            {
                AutoScroll = true,
                ForeColor = SystemColors.ControlText,
                Font = new Font("Arial", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = new Size(380, 300),
                Text = tab.name
            };

            var panel = CreateFlyoutPanel();
            var loading_text = Interface.Divider("[   Loading   ]", "");
            panel.Controls.Add(loading_text);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var controls = tab.sections.Select(CreateSection).ToArray();
                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Remove(loading_text);
                    panel.Controls.AddRange(controls);
                }));
            }).Start();


            tab_control.Controls.Add(panel);
            return tab_control;
        }

        private FlowLayoutPanel CreateSection(Section section)
        {
            switch (section.type)
            {
                case SectionType.TWEAKS:
                    return CreateTweaksSection(section);
                case SectionType.IRQPRIORITY:
                    return CreateIRQPRIORITYSection(section);
                case SectionType.DEVICES:
                    return CreateDevicesSection(section);
                case SectionType.APPX:
                    return CreateAPPXSection(section);
                case SectionType.SCHEDULED_TASKS:
                    return CreateScheduledTasksSection(section);
                default: return null;
            }
        }

        private FlowLayoutPanel CreateTweaksSection(Section section)
        {
            var panel = CreateFlyoutPanel();

            panel.Controls.Add(Interface.Divider(section.name, ""));
            foreach (var tweak in section.tweaks)
            {
                panel.Controls.Add(Interface.Tweak(tweak));
            }

            return panel;
        }

        private FlowLayoutPanel CreateIRQPRIORITYSection(Section section)
        {
            var devices_dict = IRQ.ReadDevices().ToList().OrderBy(set => set.Value.FirstOrDefault()).ToList();
            var panel = CreateFlyoutPanel();

            foreach (var pair in devices_dict)
            {
                panel.Controls.Add(Interface.Divider(pair.Key, ""));
                foreach (var irq in pair.Value)
                {
                    panel.Controls.Add(Interface.IRQPrioritySelect(irq));
                }
            }
            return panel;
        }

        private static FlowLayoutPanel CreateFlyoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(365, 0),
            };
        }

        private FlowLayoutPanel CreateAPPXSection(Section section)
        {
            var devices_dict = APPX.ALL().OrderBy(set => set);
            var panel = CreateFlyoutPanel();
            foreach (var app in devices_dict)
            {
                var tweak = new Tweak();
                tweak.name = $"Remove {app}";
                var action = TweakAction.POWERSHELL(null, $"Get-AppxPackage *{app}* | Remove-AppxPackage", null, null, null);
                tweak.actions.Add(action);
                var ta = new TweakAction();
                tweak.actions.Add(ta);
                Control c = null;
                ta.on_func = () => c.Hide();
                c = Interface.Tweak(tweak);
                panel.Controls.Add(c);
            }

            return panel;
        }

        private FlowLayoutPanel CreateScheduledTasksSection(Section section)
        {
            var devices_dict = new TaskService().AllTasks;
            var panel = CreateFlyoutPanel();

            foreach (var task in devices_dict.OrderByDescending(t => t.LastRunTime))
            {
                var tweak = new Tweak();
                tweak.name = $"Disable {task.Name}";
                tweak.description = $"Name: {task.Name}{Environment.NewLine}Path: {task.Path}{Environment.NewLine}Definition: {task.Definition}{Environment.NewLine}Task Service: {task.TaskService}{Environment.NewLine}Folder: {task.Folder}{Environment.NewLine}Last Run Time: {task.LastRunTime}{Environment.NewLine}State: {task.State}";
                var ta = new TweakAction();
                ta.on_func = () => { task.Stop(); task.Enabled = false; };
                ta.off_func = () => { task.Enabled = true; };
                ta.lookup = () => task.Enabled ? "Enabled" : "Disabled";
                ta.is_on = () => !task.Enabled;
                tweak.actions.Add(ta);
                panel.Controls.Add(Interface.Tweak(tweak));
            }

            return panel;
        }


        private FlowLayoutPanel CreateDevicesSection(Section section)
        {
            var comboBox = new ComboBox();
            comboBox.FormattingEnabled = true;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.AutoSize = true;
            comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            comboBox.Location = new Point(4, 3);
            comboBox.MinimumSize = new Size(45, 22);
            comboBox.Size = new Size(200, 22);

            Dictionary<string, List<Control>> controls_dict = new Dictionary<string, List<Control>> { };
            var power_label = "# Power Management";
            var idle_r_pin = "# IDLE R PIN";
            controls_dict.Add(power_label, new List<Control> { });
            controls_dict.Add(idle_r_pin, new List<Control> { });
            var devices = Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());

            var panel = CreateFlyoutPanel();

            panel.Controls.Add(comboBox);

            foreach (var pair in devices)
            {
                var p2 = CreateFlyoutPanel();
                foreach (var device in pair.Value)
                {
                    var p3 = CreateFlyoutPanel();
                    p3.Controls.Add(Interface.Divider(device.Name, device.FullInfo));
                    p3.Controls.Add(Interface.Tweak(Tweak.DeviceDisable(device)));

                    var deviceIdleRPIN = Tweak.DeviceIdleRPIN(device);
                    if (deviceIdleRPIN != null)
                    {
                        p3.Controls.Add(Interface.Tweak(deviceIdleRPIN));
                        controls_dict[idle_r_pin].Add(p3);
                    }

                    var enhancedPowerManagementEnabled = Tweak.EnhancedPowerManagementEnabled(device);
                    if (enhancedPowerManagementEnabled != null)
                    {
                        p3.Controls.Add(Interface.Tweak(enhancedPowerManagementEnabled));
                        controls_dict[power_label].Add(p3);
                    }

                    var MSISupported = Tweak.MsiSupported(device);
                    if (MSISupported != null)
                    {
                        p3.Controls.Add(Interface.Tweak(MSISupported));
                        //controls_dict[msi_label].Add(panel);
                    }

                    var DevicePriority = Tweak.DevicePriority(device);
                    if (DevicePriority != null)
                    {
                        p3.Controls.Add(Interface.DevicePriority(device));
                    }

                    var LinesLimit = Interface.LinesLimit(device);
                    if (LinesLimit != null)
                    {
                        p3.Controls.Add(LinesLimit);
                    }

                    var AssignmentSetOverride = Interface.AffinityOverride(device);
                    if (AssignmentSetOverride != null)
                    {
                        p3.Controls.Add(AssignmentSetOverride);
                    }
                    p3.Hide();
                    p2.Controls.Add(p3);

                }
                panel.Controls.Add(p2);
                controls_dict.Add(pair.Key, p2.Controls.Cast<Control>().ToList());
            }

            comboBox.Items.AddRange(controls_dict.Keys.ToArray());
            comboBox.SelectionChangeCommitted += (s, ee) =>
            {
                controls_dict.Values.ToList().ForEach(x => x.ForEach(p => p.Hide()));
                controls_dict[comboBox.SelectedItem.ToString()].ForEach(x => x.Show());
            };

            return panel;
        }

        private void App_Load(object sender, EventArgs e)
        {
            var tweaks_xml = "tweaks.xml";
            var xmlDocument = Parser.loadXML(tweaks_xml);
            var tabs = Parser.LoadTabs(xmlDocument);
            foreach (var tab in tabs)
            {
                var tab_control = CreateTab(tab);
                this.left_tabs.Controls.Add(tab_control);
            }
        }

        private void open_logs_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Log.log_file,
            };

            process.Start();
        }
    }
}
