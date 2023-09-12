using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
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
            InitializeComponent();
            Log.pipe += log_text_box.AppendText;
            Interface.info_box = info_box;
        }

        private TabPage CreateTab(Tab tab)
        {
            var tab_control = new TabPage();
            tab_control.AutoScroll = true;
            tab_control.ForeColor = SystemColors.ControlText;
            tab_control.Font = new Font("Arial", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            tab_control.Size = new Size(380, 300);
            tab_control.Text = tab.name;

            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(365, 0),
            };

            foreach (var section in tab.sections)
            {
                panel.Controls.Add(Interface.Divider(section.name, ""));
                var section_panel = CreateSection(section);
                if (section_panel != null)
                {
                    panel.Controls.Add(section_panel);
                }
            }

            tab_control.Controls.Add(panel);
            return tab_control;
        }

        private FlowLayoutPanel CreateSection(Section section)
        {
            switch (section.type)
            {
                case SectionType.TWEAKS:
                    return CreateTweaksSection(section.tweaks);
                case SectionType.IRQPRIORITY:
                    return CreateIRQPRIORITYSection();
                case SectionType.DEVICES:
                    return CreateDevicesSection();
                case SectionType.APPX:
                    return CreateAPPXSection();
                case SectionType.SCHEDULED_TASKS:
                    return CreateScheduledTasksSection();
                default: return null; 
            }
        }

        private FlowLayoutPanel CreateTweaksSection(List<Tweak> tweaks)
        {
            return withLoader(panel => {
                var p1 = new FlowLayoutPanel
                {
                    AutoSize = true,
                    MaximumSize = new Size(365, 0),
                };

                foreach (var tweak in tweaks)
                {
                    p1.Controls.Add(Interface.Tweak(tweak));
                }

                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Add(p1);
                }));
            });
        }

        private FlowLayoutPanel withLoader(System.Action<FlowLayoutPanel> action)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(365, 0),
            };

            var picture = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)(picture)).BeginInit();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(App));
            picture.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            picture.Location = new Point(4, 15);
            picture.Name = "pictureBox1";
            picture.Size = new System.Drawing.Size(113, 112);
            picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            picture.TabIndex = 13;
            picture.TabStop = false;
            panel.Controls.Add(picture);
            ((System.ComponentModel.ISupportInitialize)(picture)).EndInit();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                action(panel);

                picture.Invoke((MethodInvoker)(() =>
                {
                    picture.Hide();
                }));
            }).Start();

            return panel;
        }

        private FlowLayoutPanel CreateIRQPRIORITYSection()
        {
            return withLoader((panel) =>
            {
                var devices_dict = IRQ.ReadDevices().ToList().OrderBy(set => set.Value.FirstOrDefault()).ToList();
                var p1 = new FlowLayoutPanel
                {
                    AutoSize = true,
                    MaximumSize = new Size(365, 0),
                };
               
                foreach (var pair in devices_dict)
                {
                    p1.Controls.Add(Interface.Divider(pair.Key, ""));
                    foreach (var irq in pair.Value)
                    {
                        p1.Controls.Add(Interface.IRQPrioritySelect(irq));
                    }
                }

                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Add(p1);
                }));
            });
        }


        private FlowLayoutPanel CreateAPPXSection()
        {
            return withLoader((panel) =>
            {
                var devices_dict = APPX.ALL().OrderBy(set => set);
                var p1 = new FlowLayoutPanel
                {
                    AutoSize = true,
                    MaximumSize = new Size(365, 0),
                };
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
                    p1.Controls.Add(c);
                }

                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Add(p1);
                }));
            });
        }

        private FlowLayoutPanel CreateScheduledTasksSection()
        {
            return withLoader((panel) =>
            {
                var devices_dict = new TaskService().AllTasks;
                var p1 = new FlowLayoutPanel
                {
                    AutoSize = true,
                    MaximumSize = new Size(365, 0),
                };

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
                    p1.Controls.Add(Interface.Tweak(tweak));
                }

                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Add(p1);
                }));

            });
        }
       

        private FlowLayoutPanel CreateDevicesSection()
        {
            return withLoader((panel) =>
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

                var p1 = new FlowLayoutPanel
                {
                    AutoSize = true,
                    MaximumSize = new Size(365, 0),
                };

                p1.Controls.Add(comboBox);

                foreach (var pair in devices)
                {
                    var p2 = new FlowLayoutPanel();
                    p2.AutoSize = true;
                    foreach (var device in pair.Value)
                    {
                        var p3 = new FlowLayoutPanel();
                        p3.AutoSize = true;
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
                    p1.Controls.Add(p2);
                    controls_dict.Add(pair.Key, p2.Controls.Cast<Control>().ToList());
                }

                comboBox.Items.AddRange(controls_dict.Keys.ToArray());
                comboBox.SelectionChangeCommitted += (s, ee) =>
                {
                    controls_dict.Values.ToList().ForEach(x => x.ForEach(p => p.Hide()));
                    controls_dict[comboBox.SelectedItem.ToString()].ForEach(x => x.Show());
                };

                panel.Invoke((MethodInvoker)(() =>
                {
                    panel.Controls.Add(p1);
                }));
            });
        }

        private void App_Load(object sender, EventArgs e)
        {
            var tweaks_xml = "tweaks.xml";
            var xmlDocument = Parser.loadXML(tweaks_xml);
            var tabs = Parser.LoadTabs(xmlDocument);
            foreach (var tab in tabs)
            {
                var tab_control = CreateTab(tab);
                tabs_control.Controls.Add(tab_control);
            }
        }

        private void info_box_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
