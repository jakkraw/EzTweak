using CosmosKey.Utils;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EzTweak {
    public partial class App : Form {
        public App() {
            InitializeComponent();
            Log.pipe += log_text_box.AppendText;
            Interface.info_box = info_box;
        }

        private void App_Load(object sender, EventArgs e) {
            var tweaks_xml = "tweaks.xml";
            var xmlDocument = Parser.loadXML(tweaks_xml);

            foreach (var tab in Parser.LoadTabs(xmlDocument)) {
                List<System.Windows.Forms.Control> controls = new List<System.Windows.Forms.Control> { };
                foreach (var section in tab.sections) {
                    switch (section.type) {
                        case SectionType.TWEAKS: {
                                controls.Add(Interface.Divider(section.name, ""));
                                foreach (var tweak in section.tweaks) {
                                    controls.Add(Interface.Tweak(tweak));
                                }
                            }
                            break;
                        case SectionType.IRQPRIORITY: {
                                var devices_dict = IRQ.ReadDevices().ToList().OrderBy(set => set.Value.FirstOrDefault()).ToList();
                                foreach (var pair in devices_dict) {
                                    controls.Add(Interface.Divider(pair.Key, ""));
                                    foreach (var irq in pair.Value) {
                                        controls.Add(Interface.IRQPrioritySelect(irq));
                                    }
                                }
                            }
                            break;
                        case SectionType.DEVICES: {
                                var comboBox = new ComboBox();
                                comboBox.FormattingEnabled = true;
                                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                comboBox.AutoSize = true;
                                comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                                comboBox.Location = new Point(4, 3);
                                comboBox.MinimumSize = new Size(45, 22);
                                comboBox.Size = new Size(200, 22);
                                controls.Add(comboBox);
                                var devices = Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());
                                Dictionary<string, List<Control>> controls_dict = new Dictionary<string, List<Control>> { };
                                var power_label = "# Power Management";
                                var idle_r_pin = "# IDLE R PIN";
                                var msi_label = "# MSI Devices";
                                controls_dict.Add(power_label, new List<Control> { });
                                //controls_dict.Add(msi_label, new List<Control> { });
                                controls_dict.Add(idle_r_pin, new List<Control> { });
                                foreach (var pair in devices) {
                                    var cont = new List<Control>();
                                    foreach (var device in pair.Value) {
                                        var panel = new FlowLayoutPanel();
                                        panel.AutoSize = true;
                                        panel.Controls.Add(Interface.Divider(device.Name, device.FullInfo));
                                        panel.Controls.Add(Interface.Tweak(Tweak.DeviceDisable(device)));
                                        
                                        var deviceIdleRPIN = Tweak.DeviceIdleRPIN(device);
                                        if (deviceIdleRPIN != null) {
                                            panel.Controls.Add(Interface.Tweak(deviceIdleRPIN));
                                            controls_dict[idle_r_pin].Add(panel);
                                        }

                                        var enhancedPowerManagementEnabled = Tweak.EnhancedPowerManagementEnabled(device);
                                        if (enhancedPowerManagementEnabled != null) {
                                            panel.Controls.Add(Interface.Tweak(enhancedPowerManagementEnabled));
                                            controls_dict[power_label].Add(panel);
                                        }

                                        var MSISupported = Tweak.MsiSupported(device);
                                        if (MSISupported != null) {
                                            panel.Controls.Add(Interface.Tweak(MSISupported));
                                            //controls_dict[msi_label].Add(panel);
                                        }

                                        var DevicePriority = Tweak.DevicePriority(device);
                                        if (DevicePriority != null) {
                                            panel.Controls.Add(Interface.DevicePriority(device));
                                        }

                                        var LinesLimit = Interface.LinesLimit(device);
                                        if (LinesLimit != null) {
                                            panel.Controls.Add(LinesLimit);
                                        }

                                        var AssignmentSetOverride = Interface.AffinityOverride(device);
                                        if (AssignmentSetOverride != null) {
                                            panel.Controls.Add(AssignmentSetOverride);
                                        }
                                        panel.Hide();
                                        cont.Add(panel);
                                        controls.Add(panel);
                                    }
                                    controls_dict.Add(pair.Key, cont);
                                }
                                comboBox.Items.AddRange(controls_dict.Keys.ToArray());
                                comboBox.SelectionChangeCommitted += (s, ee) => {
                                    controls_dict.Values.ToList().ForEach(x => x.ForEach(p => p.Hide()));
                                    controls_dict[comboBox.SelectedItem.ToString()].ForEach(x => x.Show());
                                };
                            }
                            break;
                        case SectionType.APPX: {
                                var devices_dict = APPX.ALL().OrderBy(set => set);
                                controls.Add(Interface.Divider(section.name, ""));
                                foreach (var app in devices_dict) {
                                    var tweak = new Tweak();
                                    tweak.name = $"Remove {app}";
                                    var action = TweakAction.POWERSHELL(null, $"Get-AppxPackage *{app}* | Remove-AppxPackage", null, null, null);
                                    tweak.actions.Add(action);
                                    var ta = new TweakAction();
                                    tweak.actions.Add(ta);
                                    Control c = null;
                                    ta.on_func = () => c.Hide();
                                    c = Interface.Tweak(tweak);
                                    controls.Add(c);
                                }
                            }
                            break;
                        case SectionType.SCHEDULED_TASKS:
                            {
                                var devices_dict = new TaskService().AllTasks;
                                controls.Add(Interface.Divider(section.name, ""));
                                foreach (var task in devices_dict.OrderByDescending(t => t.LastRunTime))
                                {
                                    var tweak = new Tweak();
                                    tweak.name = $"Disable {task.Name}";
                                    tweak.description = $"Name: {task.Name}{Environment.NewLine}Path: {task.Path}{Environment.NewLine}Definition: {task.Definition}{Environment.NewLine}Task Service: {task.TaskService}{Environment.NewLine}Folder: {task.Folder}{Environment.NewLine}Last Run Time: {task.LastRunTime}{Environment.NewLine}State: {task.State}";
                                    var ta = new TweakAction();
                                    ta.on_func = () => {task.Stop(); task.Enabled = false;};
                                    ta.off_func = () => { task.Enabled = true; };
                                    ta.lookup = () => task.Enabled ? "Enabled": "Disabled";
                                    ta.is_on = () => !task.Enabled;
                                    tweak.actions.Add(ta);
                                    controls.Add(Interface.Tweak(tweak));
                                }
                            }
                            break;
                    }
                }

                if (controls.Count > 0) {
                    AddTab(tab.name, controls.ToArray());
                }
            }
        }

        private void info_box_TextChanged(object sender, EventArgs e) {

        }
    }
}
