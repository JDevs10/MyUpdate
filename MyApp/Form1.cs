using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyUpdate.Window;

namespace MyApp
{
    public partial class Form1 : Form, IMyUpdatable
    {
        private MyUpdater updater;
        public Form1()
        {
            InitializeComponent();

            this.label1.Text = this.ApplicationAssembly.GetName().Version.ToString();
            updater = new MyUpdater(this);

            updater.isUpdated();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updater.DoUpdate();
        }

        public string ApplicationName
        {
            get { return "MyApp"; }
        }

        public string ApplicationID
        {
            get { return "MyApp"; }
        }

        public Assembly ApplicationAssembly
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly(); }
        }

        public Icon ApplicationIcon
        {
            get { return this.Icon; }
        }

        public Uri UpdateXmlLocation
        {
            get { return new Uri("https://bdc.bdcloud.fr/custom/iapps/update/update.xml"); }
        }

        public Form Context
        {
            get { return this; }
        }

        public bool is_app_a_form
        {
            get { return true; }
        }
    }
}
