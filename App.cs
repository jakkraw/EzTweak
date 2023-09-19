using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;



namespace EzTweak
{
    public partial class App : Form
    {
        public App()
        {
            Log.WriteLine("EzTweak Started");
            InitializeComponent();
            info_box = description_textbox;
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
            var loading_text = Divider("[   Loading   ]", "");
            panel.Controls.Add(loading_text);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var controls = tab.sections.Select(CreateSection).ToArray();

                while (!panel.IsHandleCreated)
                    Thread.Sleep(100);

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
            var panel = CreateFlyoutPanel();
            panel.Controls.Add(Divider(section.name, ""));
            switch (section.type)
            {
                case SectionType.SECTION:
                    {
                        var p1 = CreateFlyoutPanel();
                        p1.Controls.AddRange(section.tweaks.Select(t => TweakControl(t)).ToArray());
                        panel.Controls.Add(p1);
                    }
                    break;
                case SectionType.IRQPRIORITY:
                    panel.Controls.Add(CreateIRQPRIORITYSection());
                    break;
                case SectionType.DEVICES:
                    panel.Controls.Add(CreateDevicesSection());
                    break;
                case SectionType.APPX:
                    panel.Controls.Add(CreateAPPXSection());
                    break;
                case SectionType.SCHEDULED_TASKS:
                    panel.Controls.Add(CreateScheduledTasksSection());
                    break;
                default: break;
            }

            return panel;
        }

        private FlowLayoutPanel CreateIRQPRIORITYSection()
        {
            var panel = CreateFlyoutPanel();
            panel.Controls.AddRange(IRQ_Tweak.ALL().Select(t => TweakControl(t)).ToArray());
            return panel;
        }



        private FlowLayoutPanel CreateAPPXSection()
        {
            var panel = CreateFlyoutPanel();
            foreach (var tweak in APPX_Tweak.ALL())
            {
                Control c = null;
                tweak.turn_on += () => c.Hide();
                c = TweakControl(tweak);
                panel.Controls.Add(c);
            }

            return panel;
        }

        private FlowLayoutPanel CreateScheduledTasksSection()
        {
            var devices_dict = new TaskService().AllTasks;
            var panel = CreateFlyoutPanel();

            foreach (var task in devices_dict.OrderByDescending(t => t.LastRunTime))
            {
                var tweak = new Tweak();
                tweak.name = $"{task.Name}";
                tweak.description = $"Name: {task.Name}{Environment.NewLine}Path: {task.Path}{Environment.NewLine}Definition: {task.Definition}{Environment.NewLine}Task Service: {task.TaskService}{Environment.NewLine}Folder: {task.Folder}{Environment.NewLine}Last Run Time: {task.LastRunTime}{Environment.NewLine}State: {task.State}";
                tweak.turn_on = () => { task.Stop(); task.Enabled = false; };
                tweak.turn_off = () => { task.Enabled = true; };
                tweak.status = () => task.Enabled ? "Enabled" : "Disabled";
                tweak.is_on = () => !task.Enabled;
                panel.Controls.Add(TweakControl(tweak));
            }

            return panel;
        }


        private FlowLayoutPanel CreateDevicesSection()
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
            Action<string, Control> Add = (key, control) =>
            {
                if (controls_dict.ContainsKey(key))
                {
                    controls_dict[key].Add(control);
                }
                else
                {
                    controls_dict.Add(key, new List<Control> { control });
                }
            };

            var devices = Device_Tweak.Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());
            var panel = CreateFlyoutPanel();
            var p1 = CreateFlyoutPanel();
            p1.BorderStyle = BorderStyle.Fixed3D;
            panel.Controls.Add(comboBox);
            panel.Controls.Add(p1);

            foreach (var pair in devices)
            {
                foreach (var device in pair.Value)
                {
                    var p3 = CreateFlyoutPanel();
                    p3.Controls.Add(Divider(device.Name, device.FullInfo));
                    p3.Controls.Add(TweakControl(Device_Tweak.DisableDeviceTweak(device)));

                    var deviceIdleRPIN = Device_Tweak.DeviceIdleRPIN(device);
                    if (deviceIdleRPIN != null)
                    {
                        p3.Controls.Add(TweakControl(deviceIdleRPIN));
                        Add("# IDLE R PIN", p3);
                    }
                    else
                    {
                        Add("! # IDLE R PIN", p3);
                    }

                    var enhancedPowerManagementEnabled = Device_Tweak.EnhancedPowerManagementEnabled(device);
                    if (enhancedPowerManagementEnabled != null)
                    {
                        p3.Controls.Add(TweakControl(enhancedPowerManagementEnabled));
                        Add("# Power Management", p3);
                    }
                    else
                    {
                        Add("! # Power Management", p3);
                    }

                    var MSISupported = Device_Tweak.MsiSupported(device);
                    if (MSISupported != null)
                    {
                        p3.Controls.Add(TweakControl(MSISupported));
                        Add("# MSISupported", p3);
                    }
                    else
                    {
                        Add("! # MSISupported", p3);
                    }

                    var devicePriority = Device_Tweak.DevicePriority(device);
                    if (devicePriority != null)
                    {
                        p3.Controls.Add(TweakControl(devicePriority));
                        Add("# DevicePriority", p3);
                    }
                    else
                    {
                        Add("! # DevicePriority", p3);
                    }

                    var linesLimit = Device_Tweak.LinesLimitControl(device);
                    if (linesLimit != null)
                    {
                        p3.Controls.Add(TweakControl(linesLimit));
                        Add("# LinesLimitControl", p3);
                    }
                    else
                    {
                        Add("! # LinesLimitControl", p3);
                    }

                    var AssignmentSetOverride = Device_Tweak.AssignmentSetOverride(device);
                    if (AssignmentSetOverride != null)
                    {
                        p3.Controls.Add(TweakControl(AssignmentSetOverride));
                        Add("# AssignmentSetOverride_label", p3);
                    }
                    else
                    {
                        Add("! # AssignmentSetOverride_label", p3);
                    }

                    Add(pair.Key, p3);
                }
            }

            comboBox.Items.AddRange(controls_dict.Keys.ToArray());
            comboBox.SelectionChangeCommitted += (s, ee) =>
            {
                p1.Controls.Clear();
                p1.Controls.AddRange(controls_dict[comboBox.SelectedItem.ToString()].ToArray());
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
