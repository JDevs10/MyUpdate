using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace MyUpdate.Window
{
    public class MyUpdater
    {
        /// <summary>
        /// Holds the program to-update's info
        /// </summary>
        private IMyUpdatable applicationinfo;
        /// <summary>
        /// Thread to find update
        /// </summary>
        private BackgroundWorker bgWorker;

        /// <summary>
        /// Creates a new MyUpdate object
        /// </summary>
        /// <param name="applicationinfo"></param>
        public MyUpdater(IMyUpdatable applicationinfo)
        {
            this.applicationinfo = applicationinfo;
            this.bgWorker = new BackgroundWorker();
            this.bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
        }

        /// <summary>
        /// Checks for an update for the program passed.
        /// If theree is an update, a dialog asking to download will appear.
        /// </summary>
        public void DoUpdate()
        {
            if (!this.bgWorker.IsBusy)
            {
                this.bgWorker.RunWorkerAsync(this.applicationinfo);
            }
        }

        /// <summary>
        /// A dialog / notification will appear if the application starts with updated arguments
        /// </summary>
        public void isUpdated()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            System.Console.WriteLine("GetCommandLineArgs: {0}", string.Join(", ", arguments));

            if (arguments.Length > 1 && arguments[1].Contains("newVersion-"))
            {
                MessageBox.Show(this.applicationinfo.ApplicationName + " has been updated to version " + arguments[1].Split('-')[1], this.applicationinfo.ApplicationName + " Updated ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        /// <summary>
        /// Checks for/parse update.xml on the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IMyUpdatable application = (IMyUpdatable)e.Argument;

            if (!MyUpdateXml.ExitsOnServer(application.UpdateXmlLocation))
            {
                e.Cancel = true;
            }
            else
            {
                MyUpdateXml xx = MyUpdateXml.Parse(application.UpdateXmlLocation, application.ApplicationID);

                e.Result = xx;
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                MyUpdateXml update = (MyUpdateXml)e.Result;

                if(update != null && update.IsNewerThan(this.applicationinfo.ApplicationAssembly.GetName().Version))
                {
                    if (new MyUpdateAcceptForm(this.applicationinfo, update).ShowDialog(this.applicationinfo.Context) == DialogResult.Yes)
                    {
                        this.DownloadUpdate(update);
                    }
                }

            }
        }

        private void DownloadUpdate(MyUpdateXml update)
        {
            MyUpdateDownloadForm form = new MyUpdateDownloadForm(update.Uri, update.MD5, this.applicationinfo.ApplicationIcon);
            DialogResult result = form.ShowDialog(this.applicationinfo.Context);

            if (result == DialogResult.OK)
            {
                string currentPath = this.applicationinfo.ApplicationAssembly.Location;
                string newPath = Path.GetDirectoryName(currentPath) + "\\" + update.FileName;
                /*
                System.Console.WriteLine("====================");
                System.Console.WriteLine("applicationinfo : " + Newtonsoft.Json.JsonConvert.SerializeObject(this.applicationinfo.ApplicationAssembly));
                System.Console.WriteLine("====================");
                System.Console.WriteLine("TempFilePath : " + form.TempFilePath);
                System.Console.WriteLine("currentPath : " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                System.Console.WriteLine("newPath : " + newPath);
                */

                UpdateApplicationZip(form.TempFilePath, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), update.LaunchArgs);

                Application.Exit();
            }
            else if (result == DialogResult.Abort)
            {
                MessageBox.Show("The update download was cancelled.\nThis program has not been modified.", "Update Download Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("There was a problem downloading the update.\nPlease try again later", "Update Download Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            
        }

        /// <summary>
        /// Gets and updates the files from a update.zip file and restart the new version of the application
        /// </summary>
        /// <param name="tempFilePath">The temporary downloaded update</param>
        /// <param name="currentPath">Current folder where the .exe is</param>
        /// <param name="launchArgs">Arguments (from update.xml on server) to launch with .exe file.</param>
        private void UpdateApplicationZip(string tempFilePath, string currentPath, string launchArgs)
        {
            string argument = "/C move /Y \"{0}\" \"{1}\" & del /F /Q \"{0}\" & tar -xvf \"{2}\" -C \"{3}\" & del /F /Q \"{2}\" & start \"\" \"{4}\" {5}";

            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = string.Format(argument, tempFilePath, currentPath + @"\update.zip", currentPath+@"\update.zip", currentPath, currentPath + @"\"+ this.applicationinfo.ApplicationName + ".exe", launchArgs);

            System.Console.WriteLine(info.Arguments);

            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.FileName = "cmd.exe";
            Process.Start(info);
        }

    }
}
