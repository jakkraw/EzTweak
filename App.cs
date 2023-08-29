using System;
using System.Collections.Generic;
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
                                TabControl tabs = new TabControl();
                                tabs.Location = new System.Drawing.Point(0, -10);
                                tabs.Multiline = true;
                                tabs.SelectedIndex = 0;
                                tabs.Size = new System.Drawing.Size(420, 471);
                                tabs.Padding = new System.Drawing.Point(0, 0);

                                foreach (var pair in Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList())) {
                                    var cont = new List<Control>();
                                    foreach (var device in pair.Value) {
                                        cont.Add(Interface.Divider(device.Name, device.FullInfo));
                                        cont.Add(Interface.Tweak(Tweak.DeviceDisable(device)));

                                        var deviceIdleRPIN = Tweak.DeviceIdleRPIN(device);
                                        if (deviceIdleRPIN != null) {
                                            cont.Add(Interface.Tweak(deviceIdleRPIN));
                                        }

                                        var enhancedPowerManagementEnabled = Tweak.EnhancedPowerManagementEnabled(device);
                                        if (enhancedPowerManagementEnabled != null) {
                                            cont.Add(Interface.Tweak(enhancedPowerManagementEnabled));
                                        }

                                        var MSISupported = Tweak.MsiSupported(device);
                                        if (MSISupported != null) {
                                            cont.Add(Interface.Tweak(MSISupported));
                                        }

                                        var DevicePriority = Tweak.DevicePriority(device);
                                        if (DevicePriority != null) {
                                            cont.Add(Interface.DevicePriority(device));
                                        }

                                        var LinesLimit = Interface.LinesLimit(device);
                                        if (LinesLimit != null) {
                                            cont.Add(LinesLimit);
                                        }

                                        var AssignmentSetOverride = Interface.AffinityOverride(device);
                                        if (AssignmentSetOverride != null) {
                                            cont.Add(AssignmentSetOverride);
                                        }
                                    }

                                    tabs.Controls.Add(CreateTab(pair.Key, cont.ToArray()));
                                }
                                controls.Add(tabs);
                            }
                            break;
                        case SectionType.APPX: {
                                var devices_dict = APPX.ALL().OrderBy(set => set);
                                controls.Add(Interface.Divider("Remove Windows Apps", ""));
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
