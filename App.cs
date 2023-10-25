using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EzTweak {
    public partial class App : Form {
        public App() {

            Log.WriteLine("EzTweak Started");
            InitializeComponent();
            Status.pipe += (msg) => {
                status_loading.Text = msg;
            };
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);
            Log.WriteLine("EzTweak Stopped");
        }

        private void App_Load(object sender, EventArgs e) {
           
        }
    }
}
