using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyUpdate.Console;
using System.Drawing;
using System.Reflection;
using MyUpdate;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO.Compression;
using System.ComponentModel;

namespace MyAppConsole
{
    class Program : IMyUpdatable
    {
        public string ApplicationName => "MyAppConsole";

        public string ApplicationID => "MyAppConsole";

        public Assembly ApplicationAssembly => System.Reflection.Assembly.GetExecutingAssembly();

        public Uri UpdateXmlLocation => new Uri("https://bdc.bdcloud.fr/custom/iapps/update/update.xml");

        static void Main(string[] args)
        {
            Program program = new Program();
            program.updateStatus();

            System.Console.ReadLine();
        }

        private void updateStatus()
        {
            MyUpdate.Console.MyUpdater myUpdater = new MyUpdate.Console.MyUpdater(this);
            myUpdater.DoUpdate();
        }
    }
}
