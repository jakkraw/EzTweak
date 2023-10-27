using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Action = System.Action;
using Button = System.Windows.Forms.Button;
using ComboBox = System.Windows.Forms.ComboBox;
using TextBox = System.Windows.Forms.TextBox;


namespace EzTweak
{
    public class TweakControl {
        public Toggle toogle;
        public TextBox textBox;
        public ComboBox comboBox;
        public Button on_button;
        public Button off_button;
        public Button set_button;
        public Button run_button;
        public Label name;
        public Label description;
        public Label current_value;
        public Tweak tweak;
        public Control panel;
        public LinkLabel label;
        public Control tweak_control;
        public Toggle toggle;
        public FlowLayoutPanel expand_panel;
        public TweakControl[] children = new TweakControl[] { };
        public TweakControl parent;

        public TweakControl(Tweak tweak, TweakControl parent)
        {
            this.tweak = tweak;
            

            this.parent = parent;
            if (tweak is Container_Tweak et)
            {
                children = et.tweaks.Select(t => new TweakControl(t, this)).ToArray();
            }

            panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            
            var action_panel = CreateTweakPanel();
            tweak_control = CreatePanel();
            tweak_control.SuspendLayout();
            tweak_control.Controls.Add(action_panel);
            panel.Controls.Add(tweak_control);
            tweak_control.ResumeLayout();

            Update();
            UpdateChildren();
            panel.ResumeLayout();
        }

        FlowLayoutPanel CreateTweakPanel()
        {
            var action_panel = CreateFlyoutPanel();
            action_panel.SuspendLayout();

            if (tweak.turn_on != null && tweak.turn_off != null && tweak.is_on != null)
            {
                toggle = Toggle(tweak.name, onOffClick, onOnClick, isOn);
                action_panel.Controls.Add(toggle);
            }
            else if (tweak.turn_on != null && tweak.turn_off != null)
            {
                on_button = Button("✔", onOnClick, button_size, true);
                off_button = Button("🗙", onOffClick, button_size, true);
                action_panel.Controls.Add(off_button);
                action_panel.Controls.Add(on_button);
            }
            else if (tweak.turn_on != null)
            {
                run_button = Button("✔", onRunClick, button_size, true);
                action_panel.Controls.Add(run_button);
            }
            else if (tweak.activate_value != null && tweak.valid_values != null)
            {
                comboBox = CreateComboBox();
                if (tweak.valid_values != null) comboBox.Items.AddRange(tweak.valid_values().Keys.ToArray());
                comboBox.SelectionChangeCommitted += (s, e) => onSelectChange();
                action_panel.Controls.Add(comboBox);
            }
            else if (tweak.activate_value != null && tweak.current_value != null)
            {
                textBox = CreateTextBox();
                set_button = Button("✔", onValueSet, button_size, true);
                action_panel.Controls.Add(set_button);
                action_panel.Controls.Add(textBox);
            }

            label = Label(tweak.name, onLabelClick, 40);
            action_panel.Controls.Add(label);

            action_panel.ResumeLayout();
            return action_panel;
        }

        void Update()
        {
            updateToggle();
            updateCurrentValue();
            updateComboValue();
            updateTextValue();
        }

        public void Hide()
        {
            panel.Hide();
        }

        public void Show()
        {
            panel.Show();
        }

        void UpdateParent()
        {
            if (parent != null)
            {
                parent.Update();
                parent.UpdateParent();
            }
        }

        void updateTextValue()
        {
            if (textBox != null && tweak.current_value != null) textBox.Text = tweak.current_value() ?? "";
        }

        void updateToggle()
        {
            if (toggle != null && tweak.is_on != null) toggle.Checked = tweak.is_on();
        }

        void updateComboValue()
        {
            if (comboBox != null && tweak.valid_values != null && tweak.current_value != null)
            {
                var value = tweak.current_value();
                var valid_values = tweak.valid_values();
                comboBox.SelectedIndex = comboBox.FindStringExact(valid_values.FirstOrDefault(x => x.Value == value).Key);
            }
        }

        void updateCurrentValue()
        {
            if (current_value != null) current_value.Text = tweak.current_value();
        }

        void createExpandPanel()
        {
            expand_panel = CreateExpandFlyoutPanel();
            expand_panel.SuspendLayout();
            expand_panel.BorderStyle = BorderStyle.Fixed3D;
            this.tweak.UpdateInfo();
            foreach (var pair in tweak.info)
            {
                expand_panel.Controls.Add(BoldLabel(pair.Key));
                foreach (var func in pair.Value)
                {
                    expand_panel.Controls.Add(TextLabel(func()));
                }
            }
            expand_panel.Controls.AddRange(children.Select(child => child.panel).ToArray());
            expand_panel.ResumeLayout();
        }

        void UpdateChildren()
        {
            foreach (var child in children)
            {
                child.Update();
            }
        }

        void UpdateAll()
        {
            Update();
            UpdateChildren();
            UpdateParent();
        }

        bool isOn()
        {
            return tweak.is_on != null ? tweak.is_on() : false;
        }

        string readSelection()
        {
            return tweak.valid_values().Where(o => o.Key == comboBox.SelectedItem.ToString()).First().Value;
        }

        string readText()
        {
            return textBox.Text;
        }

        void setText(string text)
        {
            tweak.activate_value(text);
        }

        void setSelection(string value)
        {
            tweak.activate_value(value);
        }

        void onValueSet()
        {
            var value = readText();
            Status.start(tweak.name, value);
            setText(value);
            Status.done(tweak.name, value);
            UpdateAll();
        }

        void onRunClick()
        {
            Status.start(tweak.name, "...");
            tweak.turn_on();
            Status.done(tweak.name, "DONE");
            UpdateAll();
        }

        void onOnClick()
        {
            Status.start(tweak.name, "ON");
            tweak.turn_on();
            Status.done(tweak.name, "ON");
            UpdateAll();
        }

        void onSelectChange()
        {
            var selection = readSelection();
            Status.start(tweak.name, selection);
            setSelection(selection);
            Status.done(tweak.name, selection);
            UpdateAll();
        }

        void onOffClick()
        {
            Status.start(tweak.name, "OFF");
            tweak.turn_off();
            Status.done(tweak.name, "OFF");
            UpdateAll();
        }

        void onLabelClick()
        {
            if(expand_panel == null)
            {
                createExpandPanel();
            }

            if (panel.Contains(expand_panel))
            {
                panel.Controls.Remove(expand_panel);
            }
            else
            {
                panel.Controls.Add(expand_panel);
            }

            Update();
            UpdateChildren();
        }

        public static int height = 25;
        public static int width = 390;
        public static Size button_size = new Size(24, 24);

        private static FlowLayoutPanel CreateFlyoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(width, 0),
                Padding = new Padding(0),
                Margin = new Padding(3)
            };
        }

        private static FlowLayoutPanel CreateExpandFlyoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                //MaximumSize = new Size(width, 0),
                //MinimumSize = new Size(width, 0),
                //Size = new Size(width, 0),
                Padding = new Padding(3),
                Margin = new Padding(3),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.Fixed3D
            };
        }

        public static Label TextLabel(string text, int offset = 0)
        {
            var label = new Label();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Padding = new Padding(3, 3, 3, 3);
            label.AutoSize = true;
            return label;
        }

        public static Label BoldLabel(string text, int offset = 0)
        {
            var label = new Label();
            label.Font = new Font("Arial", 8.5F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Padding = new Padding(3, 3, 3, 3);
            label.AutoSize = true;
            return label;
        }

        private static Panel CreatePanel()
        {
            return new Panel
            {
                Size = new Size(width, height),
                //AutoSize = true,
                MaximumSize = new Size(width, height),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }

        private static TextBox CreateTextBox()
        {
            var comboBox = new TextBox
            {
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                MinimumSize = new Size(60, 24),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            return comboBox;
        }

        public static Button Button(string text, Action on_click, Size size, bool active = false)
        {
            var btn = new Button
            {
                BackColor = active ? Color.CornflowerBlue : SystemColors.GrayText,
                Margin = new Padding(0),
                Padding = new Padding(0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = size,
                ForeColor = Color.Gainsboro,
                Text = text,
                UseVisualStyleBackColor = false
            };

            btn.Click += (x, y) => on_click();
            return btn;
        }

        public static LinkLabel Label(string text, Action on_click, int offset = 0)
        {
            var label = new LinkLabel();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Margin = new Padding(0, 0, 0, 0);
            label.Padding = new Padding(3, 3, 3, 3);
            label.LinkColor = Color.Black;
            label.AutoSize = true;
            label.MaximumSize = new Size((int)(0.75 * width), height);
            if (on_click != null)
                label.LinkClicked += (x, y) => on_click();
            return label;
        }

        public static Toggle Toggle(string name, Action off_click, Action on_click, Func<bool> is_on)
        {
            var toggle = new Toggle();
            toggle.MinimumSize = new Size((int)(0.14 * width), (int)(0.9 * height));
            toggle.AutoSize = true;
            if (is_on != null)
            {
                toggle.Checked = is_on();
            }
            toggle.OffBackColor = Color.Gray;
            toggle.OffToggleColor = Color.Gainsboro;
            toggle.OnBackColor = Color.CornflowerBlue;
            toggle.OnToggleColor = Color.WhiteSmoke;
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
                    off_click();
                }
                else
                {
                    toggle.Checked = true;
                    on_click();
                }

                toggle.Checked = is_on();
            });

            return toggle;
        }

        private static ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                FormattingEnabled = true,
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                MaximumSize = new Size(100, 22),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
        }


        private static LinkLabel CreateBigLabel(string name, Action on_click)
        {
            var label = Label(name == null ? "UNKNOWN" : $"{name.ToUpper()}", on_click);
            label.MinimumSize = new Size(width, (int)(0.9 * height));
            label.Font = new Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            return label;
        }

        private static ComboBox CreateFullComboBox()
        {
            return new ComboBox
            {
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

        public static Control Divider(string name, string description = "")
        {
            var expand_panel = CreateExpandFlyoutPanel();
            expand_panel.SuspendLayout();
            expand_panel.Controls.Add(TextLabel(name));
            expand_panel.Controls.Add(TextLabel(description));
            expand_panel.ResumeLayout();

            var tweak_panel = CreateFlyoutPanel();
            Action on_click = () => {
                if (tweak_panel.Contains(expand_panel))
                {
                    tweak_panel.Controls.Remove(expand_panel);
                }
                else
                {
                    tweak_panel.Controls.Add(expand_panel);
                }
            };

            tweak_panel.Controls.Add(CreateBigLabel(name, on_click));
            return tweak_panel;
        }

        public static FlowLayoutPanel CreateDevicesSection()
        {
            Dictionary<string, List<Control>> controls_dict = new Dictionary<string, List<Control>> { };
            Action<string, Control> Add = (key, control) => {
                if (controls_dict.ContainsKey(key))
                {
                    controls_dict[key].Add(control);
                }
                else
                {
                    controls_dict.Add(key, new List<Control> { control });
                }
            };

            var devices = WindowsResources.All_DEVICES.GroupBy(x => x.PNPClass ?? "Unknown").ToDictionary(x => x.Key, x => x.ToList());
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            var p1 = CreateExpandFlyoutPanel();
            p1.SuspendLayout();

            foreach (var pair in devices)
            {
                foreach (var device in pair.Value)
                {
                    var p3 = CreateFlyoutPanel();
                    p3.SuspendLayout();
                    p3.Controls.Add(Divider(device.Name, device.FullInfo));
                    p3.Controls.Add(new TweakControl(Device_Tweak.DisableDeviceTweak(device), null).panel);

                    var deviceIdleRPIN = Device_Tweak.DeviceIdleRPIN(device);
                    if (deviceIdleRPIN != null)
                    {
                        p3.Controls.Add(new TweakControl(deviceIdleRPIN, null).panel);
                        Add("# IDLE R PIN", p3);
                    }

                    var enhancedPowerManagementEnabled = Device_Tweak.EnhancedPowerManagementEnabled(device);
                    if (enhancedPowerManagementEnabled != null)
                    {
                        p3.Controls.Add(new TweakControl(enhancedPowerManagementEnabled, null).panel);
                        Add("# Power Management", p3);
                    }

                    var MSISupported = Device_Tweak.MsiSupported(device);
                    if (MSISupported != null)
                    {
                        p3.Controls.Add(new TweakControl(MSISupported, null).panel);
                        Add("# MSISupported", p3);
                    }

                    var devicePriority = Device_Tweak.DevicePriority(device);
                    if (devicePriority != null)
                    {
                        p3.Controls.Add(new TweakControl(devicePriority, null).panel);
                        Add("# DevicePriority", p3);
                    }

                    var linesLimit = Device_Tweak.LinesLimitControl(device);
                    if (linesLimit != null)
                    {
                        p3.Controls.Add(new TweakControl(linesLimit, null).panel);
                        Add("# LinesLimitControl", p3);
                    }

                    var AssignmentSetOverride = Device_Tweak.AssignmentSetOverride(device);
                    if (AssignmentSetOverride != null)
                    {
                        p3.Controls.Add(new TweakControl(AssignmentSetOverride, null).panel);
                        Add("# AssignmentSetOverride_label", p3);
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

    }


   

}