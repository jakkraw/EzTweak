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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(App));
            this.tabs_control = new System.Windows.Forms.TabControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.info_box = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.log_text_box = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabs_control
            // 
            this.tabs_control.Location = new System.Drawing.Point(12, 12);
            this.tabs_control.Multiline = true;
            this.tabs_control.Name = "tabs_control";
            this.tabs_control.SelectedIndex = 0;
            this.tabs_control.Size = new System.Drawing.Size(407, 554);
            this.tabs_control.TabIndex = 9;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(425, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(392, 554);
            this.tabControl1.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.info_box);
            this.tabPage1.Location = new System.Drawing.Point(4, 23);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(384, 527);
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
            this.info_box.MaximumSize = new System.Drawing.Size(385, 524);
            this.info_box.Multiline = true;
            this.info_box.Name = "info_box";
            this.info_box.ReadOnly = true;
            this.info_box.Size = new System.Drawing.Size(382, 524);
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
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(358, 365);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(218, 139);
            this.pictureBox1.TabIndex = 13;
            this.pictureBox1.TabStop = false;
            // 
            // App
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(827, 573);
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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private TabControl tabs_control;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox info_box;
        private TabPage tabPage2;
        private TextBox log_text_box;
        private PictureBox pictureBox1;
    }
}

