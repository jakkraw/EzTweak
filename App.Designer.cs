using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
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

        private TabPage CreateTab(string name, System.Windows.Forms.Control[] controls) {
            var panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.AutoSize = true;
            //panel.AutoScroll = true;
            panel.Location = new System.Drawing.Point(4, 15);
            panel.MaximumSize = new System.Drawing.Size(365, 0);
            panel.Size = new System.Drawing.Size(365, 200);
            panel.Controls.AddRange(controls);
            var tab = new System.Windows.Forms.TabPage();

            tab.AutoScroll = true;
            //tab.AutoSize = true;
            tab.MaximumSize = new System.Drawing.Size(400, 99999);
            tab.ForeColor = System.Drawing.SystemColors.ControlText;
            tab.Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tab.Location = new System.Drawing.Point(4, 22);
            tab.Padding = new System.Windows.Forms.Padding(0);
            tab.Size = new System.Drawing.Size(380, 300);
            tab.Text = name;
            //tab.Controls.AddRange(controls);
            tab.Controls.Add(panel);
            return tab;
        }

        private void AddTab(string name, System.Windows.Forms.Control[] controls) {
            var panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.AutoSize = true;
            //panel.AutoScroll = true;
            panel.Location = new System.Drawing.Point(4, 15);
            panel.MaximumSize = new System.Drawing.Size(365, 0);
            panel.Size = new System.Drawing.Size(365, 200);
            panel.Controls.AddRange(controls);
            var tab = new System.Windows.Forms.TabPage();

            tab.AutoScroll = true;
            //tab.AutoSize = true;
            tab.MaximumSize = new System.Drawing.Size(400, 99999);
            tab.ForeColor = System.Drawing.SystemColors.ControlText;
            tab.Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tab.Location = new System.Drawing.Point(4, 22);
            tab.Padding = new System.Windows.Forms.Padding(0);
            tab.Size = new System.Drawing.Size(380, 300);
            tab.Text = name;
            //tab.Controls.AddRange(controls);
            tab.Controls.Add(panel);

            this.tabs_control.Controls.Add(tab);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabs_control = new System.Windows.Forms.TabControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.info_box = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.log_text_box = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs_control
            // 
            this.tabs_control.Location = new System.Drawing.Point(35, 27);
            this.tabs_control.Multiline = true;
            this.tabs_control.Name = "tabs_control";
            this.tabs_control.SelectedIndex = 0;
            this.tabs_control.Size = new System.Drawing.Size(407, 541);
            this.tabs_control.TabIndex = 9;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(471, 18);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(392, 554);
            this.tabControl1.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.info_box);
            this.tabPage1.Location = new System.Drawing.Point(4, 23);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(591, 711);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Description";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // info_box
            // 
            this.info_box.BackColor = System.Drawing.SystemColors.Info;
            this.info_box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.info_box.Cursor = System.Windows.Forms.Cursors.Default;
            this.info_box.Location = new System.Drawing.Point(-4, 0);
            this.info_box.Multiline = true;
            this.info_box.Name = "info_box";
            this.info_box.ReadOnly = true;
            this.info_box.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.info_box.Size = new System.Drawing.Size(397, 463);
            this.info_box.TabIndex = 9;
            this.info_box.TextChanged += new System.EventHandler(this.info_box_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.log_text_box);
            this.tabPage2.Location = new System.Drawing.Point(4, 23);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(384, 527);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Logs";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // log_text_box
            // 
            this.log_text_box.BackColor = System.Drawing.SystemColors.Window;
            this.log_text_box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.log_text_box.Cursor = System.Windows.Forms.Cursors.Default;
            this.log_text_box.Location = new System.Drawing.Point(0, 0);
            this.log_text_box.Multiline = true;
            this.log_text_box.Name = "log_text_box";
            this.log_text_box.ReadOnly = true;
            this.log_text_box.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.log_text_box.Size = new System.Drawing.Size(386, 527);
            this.log_text_box.TabIndex = 8;
            this.log_text_box.WordWrap = false;
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(878, 583);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tabs_control);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "App";
            this.ShowIcon = false;
            this.Text = "EzTweak";
            this.Load += new System.EventHandler(this.App_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private TabControl tabs_control;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox info_box;
        private TabPage tabPage2;
        private TextBox log_text_box;
    }
}

