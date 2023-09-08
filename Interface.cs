using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EzTweak {
    public static class Interface {

        public static Size button_size = new Size(26, 23);
        public static Point button_location = new Point(0, 3);
        public static TextBox info_box = null;
        public static Control Button(string text, Action on_click, Point location, Size size, bool active = false) {
            var btn = new Button {
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

        public static Control Divider(string name, string description = "") {
            Action update_info = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                if (description != null && description != "") {
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

        public static LinkLabel Label(string text, Action on_click, int offset = 0) {
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

        public static CustomControls.RJControls.RJToggleButton Toggle(Action off_click, Action on_click, Func<bool> is_on) {
            var toggle = new CustomControls.RJControls.RJToggleButton();
            toggle.AutoSize = true;
            if (is_on != null) {
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
                if (toggle.Checked == active) {
                    return;
                }

                if (active) {
                    toggle.Checked = false;
                    off_click();
                } else {
                    toggle.Checked = true;
                    on_click();
                }

                toggle.Checked = is_on();
            });

            return toggle;
        }

        public static Control Tweak(string name, Action off_func, Action on_func, Func<bool> is_on_func, Func<string> get_description) {
            Action update_info = () => {
                info_box.Text = get_description();
            };
            var toggle = Toggle(off_func + update_info, on_func + update_info, is_on_func);

            Action update_toggle = () => {
                if (is_on_func != null) { toggle.Checked = is_on_func(); }
            };

            var panel = new Panel();
            panel.Size = new Size(300, 28);
            panel.Controls.Add(Label(name, update_info + update_toggle));
            if (is_on_func != null && off_func != null) {
                panel.Controls.Add(toggle);
            } else {
                if (off_func != null) {
                    panel.Controls.Add(Button("off", off_func + update_info, button_location, button_size));
                }
                var pos = button_location;
                pos.Offset(button_size.Width, 0);
                panel.Controls.Add(Button("on", on_func + update_info, pos, button_size, true));
            }

            return panel;
        }

        public static Control Tweak(Tweak tweak) {
            Func<Action, Action> wrap = f => {
                if (f == null) { return null; }

                return () => {
                    try {
                        f();
                    } catch (Exception ex) {
                        Log.WriteLine(ex.ToString());
                    }
                };
            };

            Func<Func<bool>, Func<bool>> wrap_bool = f => {
                if (f == null) { return null; }

                return () => {
                    try {
                        return f();
                    } catch (Exception ex) {
                        Log.WriteLine(ex.ToString());
                        return false;
                    }
                };
            };

            var name = tweak.name;
            var description = tweak.description;
            var off_func = tweak.actions.Count == 0 || tweak.actions.Any(a => a.off_func == null) == true ? null : tweak.actions.Select(a => wrap(a.off_func)).Aggregate((a, b) => a + b);
            var on_func = tweak.actions.Count == 0 || tweak.actions.Any(a => a.on_func == null) == true ? null : tweak.actions.Select(a => wrap(a.on_func)).Aggregate((a, b) => a + b);
            var on_description = tweak.actions.Aggregate<TweakAction, string>("", (a, b) => b.on_description == null ? a : $"{a}{b.on_description}{Environment.NewLine}");
            var off_description = tweak.actions.Aggregate<TweakAction, string>("", (a, b) => b.off_description == null ? a : $"{a}{b.off_description}{Environment.NewLine}");
            Func<string> lookup_func = () => tweak.actions.Aggregate<TweakAction, string>("", (a, b) => b.lookup == null ? a : $"{a}{b.name}=\"{b.lookup() ?? "🗑"}\"{Environment.NewLine}");
            Func<bool> is_on_func = tweak.actions.Count == 0 || tweak.actions.Any(a => a.is_on == null) == true ? (Func<bool>)null : () => !tweak.actions.Select(a => a.is_on != null ? wrap_bool(a.is_on)() : false).Any(a => a == false);
            Func<string> get_description = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                if (lookup_func != null) {
                    desc += $"⌕ Current: {Environment.NewLine}{lookup_func()}{Environment.NewLine}";
                }

                if (off_description != null && off_description != "") {
                    desc += $"❌ On Disable: {Environment.NewLine}{off_description}{Environment.NewLine}";
                }

                if (on_description != null && on_description != "") {
                    desc += $"✔ On Enable: {Environment.NewLine}{on_description}{Environment.NewLine}";
                }

                if (description != null && description != "") {
                    desc += $"📖 Description: {Environment.NewLine}{description}{Environment.NewLine}";
                }
                return desc;
            };


            return Tweak(name, off_func, on_func, is_on_func, get_description);
        }


        public static Control IRQPrioritySelect(ulong IRQ) {
            var name = $"Set IRQ {IRQ} priority";
            var reg = $@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl\IRQ{IRQ}Priority";
            var values = new Dictionary<string, string> {
                { Registry.REG_DELETE, "" },
            { "0x0", "0 (highest)" }, { "0x1", "1" }, { "0x2", "2" }, { "0x3", "3" }, { "0x4", "4" }, { "0x5", "5" },
            { "0x6", "6" }, { "0x7", "7" }, { "0x8", "8" }, { "0x9", "9" }, { "0x10", "10" }, { "0x11", "11" },
            { "0x12", "12" }, { "0x13", "13" }, { "0x14", "14" }, { "0x15", "15 (lowest)" }
            };

            Func<string> lookup_func = () => Registry.Get(reg) ?? Registry.REG_DELETE;

            Action<string> set_func = (value) => {
                if (value == null || value == Registry.REG_DELETE) {
                    Registry.Delete(reg);
                } else {
                    Registry.Set(reg, value);
                }
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                var current = lookup_func();
                desc += $"⌕ Current: {Environment.NewLine}{reg}{(current == null ? " 🗑" : $"=\"{current}\"")}{Environment.NewLine}";
                return desc;
            };
            return MultiValue(name, values, lookup_func, set_func, description_func);
        }

        public static Control DevicePriority(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var DevicePriority = $@"{reg_val}\Affinity Policy\DevicePriority";
            var name = "Device Priority";
            var values = new Dictionary<string, string> {
                { Registry.REG_DELETE, "" },
            { "0x0", "Undefined" }, { "0x1", "Low" }, { "0x2", "Medium" }, { "0x3", "High" }
            };

            Func<string> lookup_func = () => Registry.Get(DevicePriority) ?? Registry.REG_DELETE;

            Action<string> set_func = (value) => {
                if (value == null || value == Registry.REG_DELETE) {
                    Registry.Delete(DevicePriority);
                } else {
                    Registry.Set(DevicePriority, value);
                }
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                var current = lookup_func();
                desc += $"⌕ Current: {Environment.NewLine}{DevicePriority}{(current == Registry.REG_DELETE ? " 🗑" : $"=\"{current}\"")}{Environment.NewLine}";
                return desc;
            };

            return MultiValue(name, values, lookup_func, set_func, description_func);
        }

        public static Control MultiValue(string text, Dictionary<string, string> values, Func<string> lookup_func, Action<string> set_func, Func<string> description_func) {
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

        public static Control LinesLimit(Device device) {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var linesLimit = $@"{reg_val}\MessageSignaledInterruptProperties\MessageNumberLimit";
            var name = "Message Number Limit";
            Func<string> lookup_func = () => Registry.Get(linesLimit);

            if (!Registry.Exists(linesLimit)) { return null; }

            Action<string> set_func = (value) => {
                if (value == null || value == Registry.REG_DELETE || value == "") {
                    Registry.Delete(linesLimit);
                } else {
                    Registry.Set(linesLimit, value);
                }
            };


            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                var current = lookup_func();
                desc += $"⌕ Current: {Environment.NewLine}{linesLimit}{(current == Registry.REG_DELETE ? " 🗑" : $"=\"{current}\"")}{Environment.NewLine}";
                return desc;
            };

            return AnyValue(name, lookup_func, set_func, description_func);
        }
        public static Control AffinityOverride(Device device)
        {
            var reg = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{device.PnpDeviceID}\Device Parameters";
            var reg_val = $@"{reg}\Interrupt Management";
            var assignmentSetOverride = $@"{reg_val}\Affinity Policy\AssignmentSetOverride";
            var DevicePolicy = $@"{reg_val}\Affinity Policy\DevicePolicy";
            var name = "Affinity Override";

            if (!Registry.Exists(reg_val)) { return null; }
            Func<string> lookup_func = () => Registry.Get(assignmentSetOverride, Microsoft.Win32.RegistryValueKind.Binary);

            if (!Registry.Exists(assignmentSetOverride)) { return null; }

            Action<string> set_func = (value) => {
                if (value == null || value == Registry.REG_DELETE || value == "")
                {
                    Registry.Delete(assignmentSetOverride);
                    Registry.Delete(DevicePolicy);
                }
                else
                {
                    Registry.Set(assignmentSetOverride, value, Microsoft.Win32.RegistryValueKind.Binary);
                    Registry.Set(DevicePolicy, 4);
                }
            };

            Func<string> description_func = () => {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                var current = lookup_func();
                desc += $"⌕ Current: {Environment.NewLine}{assignmentSetOverride}{(current == Registry.REG_DELETE ? " 🗑" : $"=\"{current}\"")}{Environment.NewLine}";
                desc += $"{Environment.NewLine}{DevicePolicy}{(Registry.Get(DevicePolicy) == Registry.REG_DELETE ? " 🗑" : $"=\"{Registry.Get(DevicePolicy)}\"")}{Environment.NewLine}";
                return desc;
            };

            return AnyValue(name, lookup_func, set_func, description_func);
        }

        public static Control AnyValue(string text, Func<string> lookup_func, Action<string> set_func, Func<string> description_func) {
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

    }
}
