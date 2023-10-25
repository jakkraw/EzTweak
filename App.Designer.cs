﻿using Microsoft.Win32.TaskScheduler;
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
                Margin = new Padding(3)
            };
        }

        private static FlowLayoutPanel CreateExpandFlyoutPanel() {
            return new FlowLayoutPanel {
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

        

      

        public static LinkLabel Label(string text, Action on_click, int offset = 0) {
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

        public static Label TextLabel(string text, int offset = 0)
        {
            var label = new Label();
            label.Font = new Font("Arial", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label.Text = text;
            //label.Margin = new Padding(0, 6, 0, 0);
            label.Padding = new Padding(3, 3, 3, 3);
            label.AutoSize = true;
            //label.MaximumSize = new Size((int)(0.75 * width), height);
            label.SizeChanged += delegate (object sender, EventArgs e)
            {
                label.Left = (label.Parent.ClientSize.Width - label.Size.Width) / 2;
            };
            return label;
        }



        private TabPage CreateTab(Tab tab) {
            var tab_control = new TabPage {
                AutoScroll = true,
                ForeColor = SystemColors.ControlText,
                Font = new Font("Arial", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))),
                Size = new Size(380, 300),
                Text = tab.name
            };
            
            new Thread(() => {
                Thread.CurrentThread.IsBackground = true;
                var controls = tab.sections.Select(CreateSection).ToArray();
                this.Invoke((MethodInvoker)(() => {
                    
                    tab_control.Controls.AddRange(controls);
                }));
            }).Start();

            return tab_control;
        }

       

        private FlowLayoutPanel CreateSection(Section section) {
            var panel = CreateFlyoutPanel();
            panel.SuspendLayout();
            panel.Controls.Add(TweakControl.Divider(section.name, ""));
            switch (section.type) {
                case SectionType.SECTION: {
                        var tweaks = section.tweaks.Select(t => new TweakControl(t, null)).ToArray();
                        var controls = tweaks.Select(t => t.panel).ToArray();
                        panel.Controls.AddRange(controls);
                    }
                    break;
                case SectionType.IRQPRIORITY:
                    {
                        var container_tweaks = WindowsResources.All_IRQ;
                        var tweak_controls = container_tweaks.Select(t => new TweakControl(t, null)).ToArray();
                        var controls = tweak_controls.Select(t => t.panel).ToArray();
                        panel.Controls.AddRange(controls);
                    }
                    break;
                case SectionType.DEVICES:
                    {
                        panel.Controls.Add(TweakControl.CreateDevicesSection());
                    }
                    break;
                case SectionType.APPX:
                    {
                        var container_tweaks = WindowsResources.All_APPX;
                        var tweak_controls = container_tweaks.Select(t => new TweakControl(t, null)).ToArray();
                        Array.ForEach(tweak_controls, tc => tc.run_button.Click += (a, b) => tc.Hide());
                        var controls = tweak_controls.Select(t => t.panel).ToArray();
                        panel.Controls.AddRange(controls);
                    }
                    break;
                case SectionType.SCHEDULED_TASKS:
                    {
                        var tasks = WindowsResources.All_TASKS;
                        var tweak_controls = tasks.Select(tweak => new TweakControl(tweak, null)).ToArray();
                        var controls = tweak_controls.Select(t => t.panel).ToArray();
                        panel.Controls.AddRange(controls);
                    }
                    break;
                default: break;
            }
            panel.ResumeLayout();
            return panel;
        }

        

        private ToolStripItem[] CreateMenuItem(Item item)
        {
            if (item.separator)
            {
                return new[] { new ToolStripSeparator() { Size = new Size(177, 6) } };
            }

            var control = new ToolStripMenuItem
            {
                Size = new Size(180, 22),
                Text = item.name,
            };

            if(item.command_line != null)
            {
                control.Click += (a, b) => CMD_Tweak.Open(item.command_line);
            }

            var children = item.items?.SelectMany(i=> CreateMenuItem(i)).ToArray();
            if(children != null)
            {
                control.DropDownItems.AddRange(children);
            }
            
            return new[] { control };
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabs = new System.Windows.Forms.TabControl();
            this.menu = new System.Windows.Forms.MenuStrip();
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
            this.menu.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(426, 24);
            this.menu.TabIndex = 11;

            // 
            // status
            // 
            this.status.ImageScalingSize = new System.Drawing.Size(28, 28);
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
            this.status_loading.Text = "";
            // 
            // App
            // 

            var tweaks_xml = Parser.xml_file;
            var xmlDocument = Parser.loadXML(tweaks_xml);
            var tabs = Parser.LoadTweakTabs(xmlDocument);
            var items = Parser.LoadMenuItems(xmlDocument);

            foreach (var item in items)
            {
                var context_items = CreateMenuItem(item);
                this.menu.Items.AddRange(context_items);
            }

            foreach (var tab in tabs)
            {
                
                var tab_control = CreateTab(tab);
                this.tabs.Controls.Add(tab_control);
            }

            //

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
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
    }
}