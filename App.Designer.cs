using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Action = System.Action;
using Button = System.Windows.Forms.Button;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;

namespace EzTweak {
    partial class App {
        public static int height = 25;
        public static int width = 390;
        public static Size button_size = new Size((int)(0.08 * width), height);

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private static FlowLayoutPanel CreateFlyoutPanel() {
            return new FlowLayoutPanel {
                AutoSize = true,
                MaximumSize = new Size(width, 0),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

        private static FlowLayoutPanel CreateExpandFlyoutPanel() {
            return new FlowLayoutPanel {
                AutoSize = true,
                MaximumSize = new Size(width, 0),
                MinimumSize = new Size(width, 0),
                Size = new Size(width, 0),
                Padding = new Padding(0),
                Margin = new Padding(0),
                BorderStyle = BorderStyle.Fixed3D
            };
        }

        private static Panel CreatePanel() {
            return new Panel {
                Size = new Size(width, height),
                //AutoSize = true,
                MaximumSize = new Size(width, height),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

        private static ComboBox CreateComboBox() {
            return new ComboBox {
                FormattingEnabled = true,
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                MaximumSize = new Size(100, 22),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

         private static ComboBox CreateFullComboBox() {
            return new ComboBox {
                FormattingEnabled = true,
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                MaximumSize = new Size(100, 22),
                MinimumSize = new Size(width, 0),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

        public static Control Button(string text, Action on_click, Size size, bool active = false) {
            var btn = new Button {
                BackColor = active ? Color.CornflowerBlue : SystemColors.GrayText,
                Margin = new Padding(0),
                Padding = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 6.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = size,
                ForeColor = Color.Gainsboro,
                Text = text,
                UseVisualStyleBackColor = false
            };

            btn.Click += (x, y) => on_click();
            return btn;
        }

        private static LinkLabel CreateBigLabel(string name, Action on_click) {
            var label = Label(name == null ? "UNKNOWN" : $"{name.ToUpper()}", on_click);
            label.MinimumSize = new Size(width, (int)(0.9 * height));
            label.Font = new Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            return label;
        }

        public static LinkLabel Label(string text, Action on_click, int offset = 0) {
            var label = new LinkLabel();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Margin = new Padding(0, 6, 0, 0);
            label.Padding = new Padding(0, 0, 0, 0);
            label.LinkColor = Color.Black;
            label.AutoSize = true;
            label.MaximumSize = new Size((int)(0.75 * width), height);
            if (on_click != null)
                label.LinkClicked += (x, y) => on_click();
            return label;
        }

        private static TextBox CreateTextBox() {
            var comboBox = new TextBox();
            comboBox.AutoSize = true;
            comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            comboBox.MinimumSize = new Size(100, 22);
            return comboBox;
        }

        public static Toggle Toggle(string name, Action off_click, Action on_click, Func<bool> is_on) {
            var toggle = new Toggle();
            toggle.MinimumSize = new Size((int)(0.14 * width), (int)(0.9 * height));
            toggle.AutoSize = true;
            if (is_on != null) {
                toggle.Checked = is_on();
            }
            toggle.OffBackColor = Color.Gray;
            toggle.OffToggleColor = Color.Gainsboro;
            toggle.OnBackColor = Color.CornflowerBlue;
            toggle.OnToggleColor = Color.WhiteSmoke;
            toggle.UseVisualStyleBackColor = true;

            toggle.CheckedChanged += new System.EventHandler(delegate (Object o, EventArgs a) {
                var active = is_on();
                if (toggle.Checked == active) {
                    return;
                }

                if (active) {
                    toggle.Checked = false;
                    Log.WriteLine($"Turning \"{name}\" OFF...");
                    off_click();
                    Log.WriteLine($"\"{name}\" Turned OFF");
                } else {
                    toggle.Checked = true;
                    Log.WriteLine($"Turning \"{name}\" ON...");
                    on_click();
                    Log.WriteLine($"\"{name}\" Turned ON");
                }

                toggle.Checked = is_on();
            });

            return toggle;
        }

        public static Control Divider(string name, string description = "") {
            var expand_panel = CreateExpandFlyoutPanel();
            expand_panel.SuspendLayout();
            expand_panel.Controls.Add(Label(name, null));
            expand_panel.Controls.Add(Label(description, null));
            expand_panel.ResumeLayout();

            var tweak_panel = CreateFlyoutPanel();
            Action on_click = () => { 
                if (tweak_panel.Contains(expand_panel)) {
                    tweak_panel.Controls.Remove(expand_panel);
                } else {
                    tweak_panel.Controls.Add(expand_panel);
                } 
            };

            tweak_panel.Controls.Add(CreateBigLabel(name, on_click));
            return tweak_panel;
        }

        public static Control TweakControl(Tweak tweak) {
            var expand_panel = CreateExpandFlyoutPanel();
            expand_panel.SuspendLayout();
            expand_panel.BorderStyle = BorderStyle.Fixed3D;
            expand_panel.Controls.Add(Label(tweak.name, null));
            expand_panel.Controls.Add(Label(tweak.description, null));

            if (tweak is Container_Tweak container_tweak) {
                expand_panel.Controls.AddRange(container_tweak.tweaks.Select(TweakControl).ToArray());
            }
            expand_panel.ResumeLayout();

            var tweak_panel = CreateFlyoutPanel();
            tweak_panel.SuspendLayout();

            tweak.on_click = () => {
                if (tweak_panel.Contains(expand_panel)) {
                    tweak_panel.Controls.Remove(expand_panel);
                } else {
                    tweak_panel.Controls.Add(expand_panel);
                }
            };
            var tweak_control = TweakControl2(tweak);
            tweak_panel.Controls.Add(tweak_control);
            tweak_panel.ResumeLayout(false);
            return tweak_panel;
        }

        public static Control TweakControl2(Tweak tweak) {
            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on, tweak.turn_off, tweak.is_on }, o => o != null)) {
                var panel = CreatePanel();
                panel.SuspendLayout();
                var toggle = Toggle(tweak.name, tweak.turn_off, tweak.turn_on, tweak.is_on);
                Action update_toggle = () => {
                    toggle.Checked = tweak.is_on();
                };
                var p1 = CreateFlyoutPanel();
                p1.SuspendLayout();
                var label = Label(tweak.name, tweak.on_click + update_toggle);
                p1.Controls.Add(toggle);
                p1.Controls.Add(label);
                p1.ResumeLayout();
                panel.Controls.Add(p1);

                panel.ResumeLayout();
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on, tweak.turn_off }, o => o != null)) {
                var panel = CreatePanel();
                panel.SuspendLayout();
                var on_button = Button("on", tweak.turn_on, button_size, true);
                var off_button = Button("off", tweak.turn_off, button_size, true);
                var label = Label(tweak.name, tweak.on_click);
                var p1 = CreateFlyoutPanel();
                p1.SuspendLayout();
                p1.Controls.Add(off_button);
                p1.Controls.Add(on_button);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                p1.ResumeLayout();
                panel.ResumeLayout();
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on }, o => o != null)) {
                var panel = CreatePanel();
                panel.SuspendLayout();
                var p1 = CreateFlyoutPanel();
                p1.SuspendLayout();
                var button = Button("Run", tweak.turn_on, new Size((int)(0.16 * width), height), true);
                var label = Label(tweak.name, tweak.on_click);
                p1.Controls.Add(button);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                p1.ResumeLayout();
                panel.ResumeLayout();
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.activate_value, tweak.valid_values, tweak.current_value }, o => o != null)) {
                var comboBox = CreateComboBox();
                comboBox.Items.AddRange(tweak.valid_values().Values.ToArray());
                Action setSelection = () => { var x = tweak.current_value(); comboBox.SelectedIndex = comboBox.FindStringExact(tweak.valid_values()[x]); };
                Action update_info = () => { setSelection(); };

                comboBox.SelectionChangeCommitted += (s, e) => {
                    tweak.activate_value(tweak.valid_values().Where(o => o.Value == comboBox.SelectedItem.ToString()).First().Key);
                    update_info();
                };

                setSelection();
                var label = Label(tweak.name, tweak.on_click + update_info, 40);
                var panel = CreateFlyoutPanel();
                panel.SuspendLayout();
                var p1 = CreateFlyoutPanel();
                p1.SuspendLayout();
                p1.Controls.Add(comboBox);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                p1.ResumeLayout();
                panel.ResumeLayout();
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.activate_value, tweak.current_value }, o => o != null)) {
                TextBox textBox = CreateTextBox();

                Action setSelection = () => { textBox.Text = tweak.current_value() ?? ""; };
                Action update_info = () => { setSelection(); };
                setSelection();
                Action set = () => {
                    tweak.activate_value(textBox.Text);
                    update_info();
                };

                var set_button = Button("set", set, button_size, true);
                var label = Label(tweak.name, tweak.on_click + update_info, 40);
                var panel = CreateFlyoutPanel();
                panel.SuspendLayout();
                var p1 = CreateFlyoutPanel();
                p1.SuspendLayout();
                p1.Controls.Add(textBox);
                p1.Controls.Add(set_button);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                p1.ResumeLayout();
                panel.ResumeLayout();
                return panel;
            }

            throw new Exception("Unable to create controls for tweak");
        }

        private TabPage CreateTab(Tab tab) {
            var tab_control = new TabPage {
                AutoScroll = true,
                ForeColor = SystemColors.ControlText,
                Font = new Font("Arial", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = new Size(380, 300),
                Text = tab.name
            };

            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            var loading_text = Divider("Loading...", "");
            panel.Controls.Add(loading_text);

            new Thread(() => {
                Thread.CurrentThread.IsBackground = true;
                var controls = tab.sections.Select(CreateSection).ToArray();

                while (!panel.IsHandleCreated)
                    Thread.Sleep(1000);

                panel.Invoke((MethodInvoker)(() => {
                        panel.Controls.Clear();
                        panel.Controls.AddRange(controls);
                    }));
                }).Start();

            tab_control.Controls.Add(panel);
            panel.ResumeLayout();
            return tab_control;
        }

        private FlowLayoutPanel CreateSection(Section section) {
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            panel.Controls.Add(Divider(section.name, ""));
            switch (section.type) {
                case SectionType.SECTION: {
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
            panel.ResumeLayout();
            return panel;
        }

        private FlowLayoutPanel CreateIRQPRIORITYSection() {
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            panel.Controls.AddRange(IRQ_Tweak.ALL().Select(t => TweakControl(t)).ToArray());
            panel.ResumeLayout(true);
            return panel;
        }

        private FlowLayoutPanel CreateAPPXSection() {
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            foreach (var tweak in APPX_Tweak.ALL()) {
                Control c = null;
                tweak.turn_on += () => c.Hide();
                c = TweakControl(tweak);
                panel.Controls.Add(c);
            }
            panel.ResumeLayout();
            return panel;
        }

        private FlowLayoutPanel CreateScheduledTasksSection() {
            var devices_dict = new TaskService().AllTasks;
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            foreach (var task in devices_dict.OrderByDescending(t => t.LastRunTime)) {
                var tweak = new Tweak();
                tweak.name = $"{task.Name}";
                tweak.description = $"Name: {task.Name}{Environment.NewLine}Path: {task.Path}{Environment.NewLine}Definition: {task.Definition}{Environment.NewLine}Task Service: {task.TaskService}{Environment.NewLine}Folder: {task.Folder}{Environment.NewLine}Last Run Time: {task.LastRunTime}{Environment.NewLine}State: {task.State}";
                tweak.turn_on = () => { task.Stop(); task.Enabled = false; };
                tweak.turn_off = () => { task.Enabled = true; };
                tweak.status = () => task.Enabled ? "Enabled" : "Disabled";
                tweak.is_on = () => !task.Enabled;
                panel.Controls.Add(TweakControl(tweak));
            }
            panel.ResumeLayout();
            return panel;
        }

        private FlowLayoutPanel CreateDevicesSection() {
            Dictionary<string, List<Control>> controls_dict = new Dictionary<string, List<Control>> { };
            Action<string, Control> Add = (key, control) => {
                if (controls_dict.ContainsKey(key)) {
                    controls_dict[key].Add(control);
                } else {
                    controls_dict.Add(key, new List<Control> { control });
                }
            };

            var devices = Device_Tweak.Device.All().GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            var p1 = CreateExpandFlyoutPanel();
            p1.SuspendLayout();

            foreach (var pair in devices) {
                foreach (var device in pair.Value) {
                    var p3 = CreateFlyoutPanel();
                    p3.SuspendLayout();
                    p3.Controls.Add(Divider(device.Name, device.FullInfo));
                    p3.Controls.Add(TweakControl(Device_Tweak.DisableDeviceTweak(device)));

                    var deviceIdleRPIN = Device_Tweak.DeviceIdleRPIN(device);
                    if (deviceIdleRPIN != null) {
                        p3.Controls.Add(TweakControl(deviceIdleRPIN));
                        Add("# IDLE R PIN", p3);
                    } else {
                        Add("! # IDLE R PIN", p3);
                    }

                    var enhancedPowerManagementEnabled = Device_Tweak.EnhancedPowerManagementEnabled(device);
                    if (enhancedPowerManagementEnabled != null) {
                        p3.Controls.Add(TweakControl(enhancedPowerManagementEnabled));
                        Add("# Power Management", p3);
                    } else {
                        Add("! # Power Management", p3);
                    }

                    var MSISupported = Device_Tweak.MsiSupported(device);
                    if (MSISupported != null) {
                        p3.Controls.Add(TweakControl(MSISupported));
                        Add("# MSISupported", p3);
                    } else {
                        Add("! # MSISupported", p3);
                    }

                    var devicePriority = Device_Tweak.DevicePriority(device);
                    if (devicePriority != null) {
                        p3.Controls.Add(TweakControl(devicePriority));
                        Add("# DevicePriority", p3);
                    } else {
                        Add("! # DevicePriority", p3);
                    }

                    var linesLimit = Device_Tweak.LinesLimitControl(device);
                    if (linesLimit != null) {
                        p3.Controls.Add(TweakControl(linesLimit));
                        Add("# LinesLimitControl", p3);
                    } else {
                        Add("! # LinesLimitControl", p3);
                    }

                    var AssignmentSetOverride = Device_Tweak.AssignmentSetOverride(device);
                    if (AssignmentSetOverride != null) {
                        p3.Controls.Add(TweakControl(AssignmentSetOverride));
                        Add("# AssignmentSetOverride_label", p3);
                    } else {
                        Add("! # AssignmentSetOverride_label", p3);
                    }
                    p3.ResumeLayout();
                    Add(pair.Key, p3);
                }
            }
            var comboBox = CreateFullComboBox();
            comboBox.Items.AddRange(controls_dict.Keys.ToArray());
            comboBox.SelectionChangeCommitted += (s, ee) => {
                p1.Controls.Clear();
                p1.Controls.AddRange(controls_dict[comboBox.SelectedItem.ToString()].ToArray());
            };
            panel.Controls.Add(comboBox);
            panel.Controls.Add(p1);
            p1.ResumeLayout(false);
            panel.ResumeLayout();
            return panel;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabs = new System.Windows.Forms.TabControl();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.menu_open = new System.Windows.Forms.ToolStripMenuItem();
            this.status = new System.Windows.Forms.StatusStrip();
            this.status_loading = new System.Windows.Forms.ToolStripStatusLabel();
            this.menu.SuspendLayout();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Location = new System.Drawing.Point(0, 24);
            this.tabs.Margin = new System.Windows.Forms.Padding(0);
            this.tabs.Multiline = true;
            this.tabs.Name = "tabs";
            this.tabs.Padding = new System.Drawing.Point(0, 0);
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(426, 472);
            this.tabs.TabIndex = 9;
            // 
            // menu
            // 
            this.menu.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_open});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(426, 24);
            this.menu.TabIndex = 11;
            // 
            // menu_open
            // 
            this.menu_open.Name = "menu_open";
            this.menu_open.Size = new System.Drawing.Size(48, 20);
            this.menu_open.Text = "Open";
            // 
            // status
            // 
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_loading});
            this.status.Location = new System.Drawing.Point(0, 496);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(426, 22);
            this.status.TabIndex = 12;
            // 
            // status_loading
            // 
            this.status_loading.Name = "status_loading";
            this.status_loading.Size = new System.Drawing.Size(59, 17);
            this.status_loading.Text = "Loading...";
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(426, 518);
            this.Controls.Add(this.status);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.menu);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::EzTweak.Properties.Resources.icon;
            this.Name = "App";
            this.Text = "EzTweak";
            this.Load += new System.EventHandler(this.App_Load);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TabControl tabs;
        private StatusStrip status;
        private ToolStripStatusLabel status_loading;
        private MenuStrip menu;
        private ToolStripMenuItem menu_open;
    }
}