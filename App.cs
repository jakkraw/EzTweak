using System;
using System.Windows.Forms;

namespace EzTweak {
    public partial class App : Form {
        public App() {
            Log.WriteLine("EzTweak Started");
            InitializeComponent();
            Status.pipe += (msg) => {
                status_loading.Text = msg;
            };
            Log.WriteLine($"User: {WindowsSystem.GetUserType()}");
            status_user.Text = $"👤 {WindowsSystem.GetUserType()}";
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);
            Log.WriteLine("EzTweak Stopped");
        }

        private void App_Load(object sender, EventArgs e) {
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

            foreach (var item in TweakControl.tweakContols)
                item.Update();
            
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
