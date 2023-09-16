using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EzTweak {
    partial class App {
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

        private static FlowLayoutPanel CreateFlyoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(365, 0),
            };
        }

        public static Size button_size = new Size(26, 23);
        public static Point button_location = new Point(0, 3);
        public static TextBox info_box = null;
        public static Control Button(string text, Action on_click, Point location, Size size, bool active = false)
        {
            var btn = new Button
            {
                BackColor = active ? Color.MediumSlateBlue : SystemColors.GrayText,
                Location = location,
                Margin = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 6.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = size,
                ForeColor = Color.Gainsboro,
                Text = text,
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += new EventHandler(delegate (object o, EventArgs a) {
                on_click();
            });
            return btn;
        }

        public static Control Divider(string name, string description = "")
        {
            Action update_info = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                if (description != null && description != "")
                {
                    desc += $"📖 Description: {Environment.NewLine}{description}{Environment.NewLine}";
                }

                info_box.Text = $"{name}{Environment.NewLine}{Environment.NewLine}{desc}";
            };

            var label = Label(name == null ? "UNKNOWN" : $"{name.ToUpper()}", update_info);
            label.AutoSize = false;
            label.Location = new Point(2, 7);
            label.Size = new Size(340, 28);
            label.LinkColor = Color.Black;
            label.Font = new Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

            var panel = new Panel();
            panel.Size = new Size(340, 28);
            panel.Controls.Add(label);
            return panel;
        }

        public static LinkLabel Label(string text, Action on_click, int offset = 0)
        {
            var label = new LinkLabel();
            label.AutoSize = true;
            label.Location = new Point(60 + offset, 7);
            label.LinkColor = Color.Black;
            label.Text = text;
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(delegate (object o, LinkLabelLinkClickedEventArgs a) {
                on_click();
            });
            return label;
        }

        public static Toggle Toggle(string name, Action off_click, Action on_click, Func<bool> is_on)
        {
            var toggle = new Toggle();
            toggle.AutoSize = true;
            if (is_on != null)
            {
                toggle.Checked = is_on();
            }
            toggle.Location = new Point(4, 3);
            toggle.MinimumSize = new Size(45, 22);
            toggle.OffBackColor = Color.Gray;
            toggle.OffToggleColor = Color.Gainsboro;
            toggle.OnBackColor = Color.MediumSlateBlue;
            toggle.OnToggleColor = Color.WhiteSmoke;
            toggle.Size = new Size(45, 22);
            toggle.UseVisualStyleBackColor = true;

            toggle.CheckedChanged += new System.EventHandler(delegate (Object o, EventArgs a) {
                var active = is_on();
                if (toggle.Checked == active)
                {
                    return;
                }

                if (active)
                {
                    toggle.Checked = false;
                    Log.WriteLine($"Turning \"{name}\" OFF...");
                    off_click();
                    Log.WriteLine($"\"{name}\" Turned OFF");
                }
                else
                {
                    toggle.Checked = true;
                    Log.WriteLine($"Turning \"{name}\" ON...");
                    on_click();
                    Log.WriteLine($"\"{name}\" Turned ON");
                }

                toggle.Checked = is_on();
            });

            return toggle;
        }

        public static Control TweakControl(string name, Action off_func, Action on_func, Func<bool> is_on_func, Func<string> get_description)
        {
            Action update_info = () => {
                info_box.Text = get_description();
            };
            var toggle = Toggle(name, off_func + update_info, on_func + update_info, is_on_func);

            Action update_toggle = () => {
                if (is_on_func != null) { toggle.Checked = is_on_func(); }
            };

            var panel = new Panel();
            panel.Size = new Size(300, 28);
            panel.Controls.Add(Label(name, update_info + update_toggle));
            if (is_on_func != null && off_func != null)
            {
                panel.Controls.Add(toggle);
            }
            else
            {
                if (off_func != null)
                {
                    panel.Controls.Add(Button("off", off_func + update_info, button_location, button_size));
                }
                var pos = button_location;
                pos.Offset(button_size.Width, 0);
                panel.Controls.Add(Button("on", on_func + update_info, pos, button_size, true));
            }

            return panel;
        }

        public static Control TweakControl(Tweak tweak)
        {
            Func<Action, Action> wrap = f => {
                if (f == null) { return null; }

                return () => {
                    try
                    {
                        f();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"{tweak.name} {Thread.CurrentThread.ManagedThreadId} {ex.ToString()}");
                    }
                };
            };

            Func<Func<bool>, Func<bool>> wrap_bool = f => {
                if (f == null) { return null; }

                return () => {
                    try
                    {
                        return f();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"{tweak.name} {Thread.CurrentThread.ManagedThreadId} {ex.ToString()}");
                        return false;
                    }
                };
            };

            var name = tweak.name;
            var description = tweak.description;
            var off_func = tweak.tweaks.Count == 0 || tweak.tweaks.Any(a => a.off_func == null) == true ? null : tweak.tweaks.Select(a => wrap(a.off_func)).Aggregate((a, b) => a + b);
            var on_func = tweak.tweaks.Count == 0 || tweak.tweaks.Any(a => a.on_func == null) == true ? null : tweak.tweaks.Select(a => wrap(a.on_func)).Aggregate((a, b) => a + b);
            var on_description = tweak.tweaks.Aggregate<Tweak, string>("", (a, b) => b.on_description == null ? a : $"{a}{b.on_description}{Environment.NewLine}");
            var off_description = tweak.tweaks.Aggregate<Tweak, string>("", (a, b) => b.off_description == null ? a : $"{a}{b.off_description}{Environment.NewLine}");
            Func<string> lookup_func = () => tweak.tweaks.Aggregate<Tweak, string>("", (a, b) => b.lookup == null ? a : $"{a}{b.name}=\"{b.lookup()}\"{Environment.NewLine}");
            Func<bool> is_on_func = tweak.tweaks.Count == 0 || tweak.tweaks.Any(a => a.is_on == null) == true ? (Func<bool>)null : () => !tweak.tweaks.Select(a => a.is_on != null ? wrap_bool(a.is_on)() : false).Any(a => a == false);
            Func<string> get_description = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                if (lookup_func != null)
                {
                    desc += $"⌕ Current: {Environment.NewLine}{lookup_func()}{Environment.NewLine}";
                }

                if (off_description != null && off_description != "")
                {
                    desc += $"❌ On Disable: {Environment.NewLine}{off_description}{Environment.NewLine}";
                }

                if (on_description != null && on_description != "")
                {
                    desc += $"✔ On Enable: {Environment.NewLine}{on_description}{Environment.NewLine}";
                }

                if (description != null && description != "")
                {
                    desc += $"📖 Description: {Environment.NewLine}{description}{Environment.NewLine}";
                }
                return desc;
            };


            return TweakControl(name, off_func, on_func, is_on_func, get_description);
        }

        public static Control TweakControl(Tk tweak)
        {
            Func<string> desc = () =>
            {
                return $"{tweak.description}{Environment.NewLine}{tweak.status()}";
            };
            return TweakControl(tweak.name, tweak.turn_off, tweak.turn_on, tweak.is_on, desc);
        }


        public static Control IRQPrioritySelectControl(ulong IRQ)
        {
            var name = $"Set IRQ {IRQ} priority";
            var reg = $@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl\IRQ{IRQ}Priority";
            var values = new Dictionary<string, string> {
                { Registry.REG_DELETE, Registry.REG_DELETE },
            { "0x0", "0 (highest)" }, { "0x1", "1" }, { "0x2", "2" }, { "0x3", "3" }, { "0x4", "4" }, { "0x5", "5" },
            { "0x6", "6" }, { "0x7", "7" }, { "0x8", "8" }, { "0x9", "9" }, { "0x10", "10" }, { "0x11", "11" },
            { "0x12", "12" }, { "0x13", "13" }, { "0x14", "14" }, { "0x15", "15 (lowest)" }
            };

            Func<string> lookup_func = () => Registry.From_DWORD(Registry.Get_DWORD(reg));

            Action<string> set_func = (value) => {
                Registry.Set_DWORD(reg, Registry.To_DWORD(value));
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                desc += $"⌕ Current: {Environment.NewLine}{reg}{lookup_func()}{Environment.NewLine}";
                return desc;
            };
            return MultiValueControl(name, values, lookup_func, set_func, description_func);
        }

        public static Control DevicePriorityControl(Device device)
        {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var DevicePriority = $@"{reg_val}\Affinity Policy\DevicePriority";
            var name = "Device Priority";
            var values = new Dictionary<string, string> {
                { Registry.REG_DELETE, Registry.REG_DELETE },
            { "0x0", "Undefined" }, { "0x1", "Low" }, { "0x2", "Medium" }, { "0x3", "High" }
            };

            Func<string> lookup_func = () => Registry.From_DWORD(Registry.Get_DWORD(DevicePriority));

            Action<string> set_func = (value) => {
                Registry.Set_DWORD(DevicePriority, Registry.To_DWORD(value));
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                desc += $"⌕ Current: {Environment.NewLine}{DevicePriority}{lookup_func()}{Environment.NewLine}";
                return desc;
            };

            return MultiValueControl(name, values, lookup_func, set_func, description_func);
        }

        public static Control MultiValueControl(string text, Dictionary<string, string> values, Func<string> lookup_func, Action<string> set_func, Func<string> description_func)
        {
            var comboBox = new ComboBox();
            comboBox.FormattingEnabled = true;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.AddRange(values.Values.ToArray());
            comboBox.AutoSize = true;
            comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            comboBox.Location = new Point(4, 3);
            comboBox.MinimumSize = new Size(45, 22);
            comboBox.Size = new Size(80, 22);
            Action setSelection = () => { var x = lookup_func(); comboBox.SelectedIndex = comboBox.FindStringExact(values[x]); };
            Action update_info = () => { info_box.Text = description_func(); setSelection(); };

            comboBox.SelectionChangeCommitted += (s, e) => {
                set_func(values.Where(o => o.Value == comboBox.SelectedItem.ToString()).First().Key);
                update_info();
            };

            setSelection();
            var label = Label(text, update_info, 40);
            var panel = new Panel();
            panel.Controls.Add(comboBox);
            panel.Controls.Add(label);
            panel.Size = new Size(300, 28);
            return panel;
        }

        public static Control LinesLimitControl(Device device)
        {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var linesLimit = $@"{reg_val}\MessageSignaledInterruptProperties\MessageNumberLimit";
            var name = "Message Number Limit";
            Func<string> lookup_func = () => Registry.From_DWORD(Registry.Get_DWORD(linesLimit));

            if (!Registry.Exists(linesLimit)) { return null; }

            Action<string> set_func = (value) => {
                Registry.Set_DWORD(linesLimit, Registry.To_DWORD(value));
            };


            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                desc += $"⌕ Current: {Environment.NewLine}{linesLimit}{lookup_func()}{Environment.NewLine}";
                return desc;
            };

            return AnyValueControl(name, lookup_func, set_func, description_func);
        }
        public static Control AffinityOverrideControl(Device device)
        {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var assignmentSetOverride = $@"{reg_val}\Affinity Policy\AssignmentSetOverride";
            var DevicePolicy = $@"{reg_val}\Affinity Policy\DevicePolicy";
            var name = "Affinity Override";

            if (!Registry.Exists(reg_val)) { return null; }
            Func<string> lookup_func = () => Registry.From_BINARY(Registry.Get_BINARY(assignmentSetOverride));

            if (!Registry.Exists(assignmentSetOverride)) { return null; }

            Action<string> set_func = (value) => {
                Registry.Set_BINARY(assignmentSetOverride, Registry.To_BINARY(value));
                Registry.Set_DWORD(DevicePolicy, 4);
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                var current = lookup_func();
                desc += $"⌕ Current: {Environment.NewLine}{Registry.From_BINARY(Registry.Get_BINARY(assignmentSetOverride))}{Environment.NewLine}";
                desc += $"{Environment.NewLine}{DevicePolicy}=\"{Registry.From_DWORD(Registry.Get_DWORD(DevicePolicy))}\"{Environment.NewLine}";
                return desc;
            };

            return AnyValueControl(name, lookup_func, set_func, description_func);
        }

        public static Control AnyValueControl(string text, Func<string> lookup_func, Action<string> set_func, Func<string> description_func)
        {
            var comboBox = new TextBox();
            comboBox.AutoSize = true;
            comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            comboBox.Location = new Point(4, 3);
            comboBox.MinimumSize = new Size(45, 22);
            comboBox.Size = new Size(80, 22);

            Action setSelection = () => { comboBox.Text = lookup_func() ?? ""; };
            Action update_info = () => { info_box.Text = description_func(); setSelection(); };
            setSelection();
            comboBox.TextChanged += (s, e) => {
                set_func(comboBox.Text);
                update_info();
            };


            var label = Label(text, update_info, 40);
            var panel = new Panel();
            panel.Controls.Add(comboBox);
            panel.Controls.Add(label);
            panel.Size = new Size(300, 28);
            return panel;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.left_tabs = new System.Windows.Forms.TabControl();
            this.description_textbox = new System.Windows.Forms.TextBox();
            this.open_logs_label = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // left_tabs
            // 
            this.left_tabs.Location = new System.Drawing.Point(3, 3);
            this.left_tabs.Multiline = true;
            this.left_tabs.Name = "left_tabs";
            this.left_tabs.SelectedIndex = 0;
            this.left_tabs.Size = new System.Drawing.Size(407, 536);
            this.left_tabs.TabIndex = 9;
            // 
            // description_textbox
            // 
            this.description_textbox.BackColor = System.Drawing.SystemColors.Info;
            this.description_textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.description_textbox.Cursor = System.Windows.Forms.Cursors.Default;
            this.description_textbox.Location = new System.Drawing.Point(416, 3);
            this.description_textbox.MaximumSize = new System.Drawing.Size(385, 524);
            this.description_textbox.Multiline = true;
            this.description_textbox.Name = "description_textbox";
            this.description_textbox.ReadOnly = true;
            this.description_textbox.Size = new System.Drawing.Size(346, 524);
            this.description_textbox.TabIndex = 9;
            // 
            // open_logs_label
            // 
            this.open_logs_label.AutoSize = true;
            this.open_logs_label.Location = new System.Drawing.Point(731, 538);
            this.open_logs_label.Name = "open_logs_label";
            this.open_logs_label.Size = new System.Drawing.Size(31, 14);
            this.open_logs_label.TabIndex = 10;
            this.open_logs_label.TabStop = true;
            this.open_logs_label.Text = "Logs";
            this.open_logs_label.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.open_logs_label_LinkClicked);
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.open_logs_label);
            this.Controls.Add(this.description_textbox);
            this.Controls.Add(this.left_tabs);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "App";
            this.ShowIcon = false;
            this.Text = "EzTweak";
            this.Load += new System.EventHandler(this.App_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TabControl left_tabs;
        private TextBox description_textbox;
        private LinkLabel open_logs_label;
    }
}

