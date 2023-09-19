using Hardware.Info;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace EzTweak
{
    partial class App
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private static FlowLayoutPanel CreateFlyoutPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(width, 0),
            };
        }

        private static FlowLayoutPanel CreateFlyoutOptionPanel()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(width, 0),
            };
        }
        private static Panel CreatePanel()
        {
            return new Panel
            {
                Size = new Size(width, height),
            };
        }

        public static int height = 25;
        public static int width = 380;
        public static Size button_size = new Size((int)(0.08 * width), height);
        public static TextBox info_box = null;
        public static Control Button(string text, Action on_click, Size size, bool active = false)
        {
            var btn = new Button
            {
                BackColor = active ? Color.MediumSlateBlue : SystemColors.GrayText,
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

        public static Control Divider(string name, string description = "")
        {
            Action update_info = () =>
            {
                var desc = $"⚡ {name} ⚡{Environment.NewLine}{Environment.NewLine}";
                if (description != null && description != "")
                {
                    desc += $"📖 Description: {Environment.NewLine}{description}{Environment.NewLine}";
                }

                info_box.Text = $"{name}{Environment.NewLine}{Environment.NewLine}{desc}";
            };

            var label = Label(name == null ? "UNKNOWN" : $"{name.ToUpper()}", update_info);
            label.MinimumSize = new Size(width, (int)(0.9 * height));
            label.Font = new Font("Arial", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            return label;
        }

        public static LinkLabel Label(string text, Action on_click, int offset = 0)
        {
            var label = new LinkLabel();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            label.Margin = new Padding(0, 6, 0, 0);
            label.LinkColor = Color.Black;
            //label.Location = new Point(60 + offset, 7);
            label.AutoSize = true;
            label.MaximumSize = new Size((int)(0.75 * width), height);
            label.LinkClicked += (x, y) => on_click();
            return label;
        }

        public static Toggle Toggle(string name, Action off_click, Action on_click, Func<bool> is_on)
        {
            var toggle = new Toggle();
            toggle.MinimumSize = new Size((int)(0.14 * width), (int)(0.9 * height));
            toggle.AutoSize = true;
            //toggle.Size = new Size(45, height);
            if (is_on != null)
            {
                toggle.Checked = is_on();
            }
            //toggle.Location = new Point(4, 3);
            //toggle.MinimumSize = new Size(45, 22);
            toggle.OffBackColor = Color.Gray;
            toggle.OffToggleColor = Color.Gainsboro;
            toggle.OnBackColor = Color.MediumSlateBlue;
            toggle.OnToggleColor = Color.WhiteSmoke;
            //toggle.Size = new Size(45, height);
            toggle.UseVisualStyleBackColor = true;

            toggle.CheckedChanged += new System.EventHandler(delegate (Object o, EventArgs a)
            {
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


        public static Control TweakControl(Tweak tweak)
        {
           
            if (tweak is Container_Tweak container_tweak)
            {
                var panel = CreateFlyoutPanel();
                foreach (var item in container_tweak.tweaks)
                {
                    panel.Controls.Add(TweakControl(item));
                    
                }
                panel.Visible = false;
                panel.BorderStyle = BorderStyle.Fixed3D;
                tweak.on_click = () => { panel.Visible = !panel.Visible; };
                var panel2 = CreateFlyoutPanel();
                var c = TweakControl2(tweak);
                panel2.Controls.Add(c);
                panel2.Controls.Add(panel);
                return panel2;
            }

            return TweakControl2(tweak);
        }

        public static Control TweakControl2(Tweak tweak)
        {
            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on, tweak.turn_off, tweak.is_on }, o => o != null))
            {
                Action update_info = () =>
                {
                    info_box.Text = tweak.description;
                };
                var panel = CreatePanel();
                var toggle = Toggle(tweak.name, tweak.turn_off + update_info, tweak.turn_on + update_info, tweak.is_on);

                Action update_toggle = () =>
                {
                    toggle.Checked = tweak.is_on();
                };

                var p1 = CreateFlyoutPanel();
                var label = Label(tweak.name, tweak.on_click + update_info + update_toggle );
                p1.Controls.Add(toggle);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on, tweak.turn_off }, o => o != null))
            {
                Action update_info = () => { info_box.Text = tweak.description; };
                var panel = CreatePanel();

                var on_button = Button("on", tweak.turn_on + update_info, button_size, true);
                var off_button = Button("off", tweak.turn_off + update_info, button_size, true);
                var label = Label(tweak.name, tweak.on_click + update_info);

                var p1 = CreateFlyoutPanel();
                p1.Controls.Add(off_button);
                p1.Controls.Add(on_button);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.turn_on }, o => o != null))
            {
                Action update_info = () =>
                {
                    info_box.Text = tweak.description;
                };
                var panel = CreatePanel();

                var p1 = CreateFlyoutPanel();
                var button = Button("Run", tweak.turn_on + update_info, new Size((int)(0.16 * width), height), true);
                var label = Label(tweak.name, tweak.on_click + update_info);
                p1.Controls.Add(button);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.activate_value, tweak.valid_values, tweak.current_value }, o => o != null))
            {
                var comboBox = new ComboBox();
                comboBox.FormattingEnabled = true;
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.Items.AddRange(tweak.valid_values().Values.ToArray());
                comboBox.AutoSize = true;
                comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                comboBox.Location = new Point(4, 3);
                comboBox.MinimumSize = new Size(45, 22);
                comboBox.Size = new Size(80, 22);
                Action setSelection = () => { var x = tweak.current_value(); comboBox.SelectedIndex = comboBox.FindStringExact(tweak.valid_values()[x]); };
                Action update_info = () => { info_box.Text = tweak.description; setSelection(); };

                comboBox.SelectionChangeCommitted += (s, e) =>
                {
                    tweak.activate_value(tweak.valid_values().Where(o => o.Value == comboBox.SelectedItem.ToString()).First().Key);
                    update_info();
                };

                setSelection();
                var label = Label(tweak.name, tweak.on_click + update_info, 40);
                var panel = CreateFlyoutPanel();
                var p1 = CreateFlyoutPanel();
                p1.Controls.Add(comboBox);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                return panel;
            }

            if (Array.TrueForAll(new Object[] { tweak.name, tweak.description, tweak.activate_value, tweak.current_value }, o => o != null))
            {
                var comboBox = new TextBox();
                comboBox.AutoSize = true;
                comboBox.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                comboBox.Location = new Point(4, 3);
                comboBox.MinimumSize = new Size(45, 22);
                comboBox.Size = new Size(80, 22);

                Action setSelection = () => { comboBox.Text = tweak.current_value() ?? ""; };
                Action update_info = () => { info_box.Text = tweak.description; setSelection(); };
                setSelection();
                Action set = () =>
                {
                    tweak.activate_value(comboBox.Text);
                    update_info();
                };

                var set_button = Button("set", set, button_size, true);
                var label = Label(tweak.name, tweak.on_click + update_info, 40);
                var panel = CreateFlyoutPanel();
                var p1 = CreateFlyoutPanel();
                p1.Controls.Add(set_button);
                p1.Controls.Add(comboBox);
                p1.Controls.Add(label);
                panel.Controls.Add(p1);
                return panel;
            }

            return null;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
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

