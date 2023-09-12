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

