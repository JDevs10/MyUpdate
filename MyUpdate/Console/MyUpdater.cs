using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace MyUpdate.Console
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

        private MyUpdateXml myUpdateXml;
        private Uri location;
        private string tempFile = Path.GetTempFileName();
        private string md5;

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
                System.Console.WriteLine(this.applicationinfo.ApplicationName + " Updated - " + this.applicationinfo.ApplicationName + " has been updated to version " + arguments[1].Split('-')[1]);
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

                if (update != null && update.IsNewerThan(this.applicationinfo.ApplicationAssembly.GetName().Version))
                {
                    this.DownloadUpdate(update);
                }

            }
        }

        private void DownloadUpdate(MyUpdateXml updateXml)
        {
            this.location = updateXml.Uri;
            this.md5 = updateXml.MD5;
            this.myUpdateXml = updateXml;

            WebClient webClient = new WebClient();
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);

            webClient.DownloadFileAsync(this.location, this.tempFile);
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            System.Console.WriteLine(" - " + e.ProgressPercentage + " | " + String.Format("Download {0} of {1}", FormatByBytes(e.BytesReceived, 1, true), FormatByBytes(e.TotalBytesToReceive, 1, true)));
        }

        private string FormatByBytes(long bytes, int decimalPlaces, bool showByteType)
        {
            double newBytes = bytes;
            string formatString = "{0";
            string byteType = "B";

            if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                newBytes /= 1048576;
                byteType = "MB";
            }
            else
            {
                newBytes /= 1073741824;
                byteType = "GB";
            }

            if (decimalPlaces > 0)
            {
                formatString += ":0.";
            }

            for (int i = 0; i < decimalPlaces; i++) { }
            {
                formatString += "0";
            }

            formatString += "}";


            if (showByteType)
            {
                formatString += byteType;
            }

            return string.Format(formatString, newBytes);
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (File.Exists(this.tempFile))
                {
                    File.Delete(this.tempFile);
                }
                System.Console.WriteLine("Update Download Error - There was a problem downloading the update. Please try again later.");
                return;
            }
            else if (e.Cancelled)
            {
                if (File.Exists(this.tempFile))
                {
                    File.Delete(this.tempFile);
                }
                System.Console.WriteLine("Update Download Cancelled - The update download was cancelled. This program has not been modified.");
                return;
            }
            else
            {
                System.Console.WriteLine("Verifying Download...");

                //bgWorker.RunWorkerAsync(new string[] { this.tempFile, this.md5 });

                string filemd5 = Hasher.HashFile(this.tempFile, HashType.MD5);
                string updatemd5 = this.md5.ToLower();

                System.Console.WriteLine("tempFile : " + this.tempFile);
                System.Console.WriteLine("filemd5 : " + filemd5);
                System.Console.WriteLine("HashType : " + HashType.MD5);
                System.Console.WriteLine("updatemd5 : " + updatemd5);

                if (filemd5 != updatemd5)
                {
                    System.Console.WriteLine("Update Download Error - There was a problem downloading the update. Please try again later.");
                    return;
                }
                else
                {
                    string currentPath = this.applicationinfo.ApplicationAssembly.Location;
                    string newPath = Path.GetDirectoryName(currentPath) + "\\" + this.myUpdateXml.FileName;

                    System.Console.WriteLine("====================");
                    System.Console.WriteLine("applicationinfo : " + Newtonsoft.Json.JsonConvert.SerializeObject(this.applicationinfo.ApplicationAssembly));
                    System.Console.WriteLine("====================");
                    System.Console.WriteLine("TempFilePath : " + this.tempFile);
                    System.Console.WriteLine("currentPath : " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                    System.Console.WriteLine("newPath : " + newPath);


                    UpdateApplicationZip(this.tempFile, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), myUpdateXml.LaunchArgs);

                    // Application.Exit();
                }

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
            try
            {
                string argument = "/C move /Y \"{0}\" \"{1}\" & del /F /Q \"{0}\" & tar -xvf \"{2}\" -C \"{3}\" & del /F /Q \"{2}\" & start \"\" \"{4}\" {5}";

                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = string.Format(argument, tempFilePath, currentPath + @"\update.zip", currentPath + @"\update.zip", currentPath, currentPath + @"\" + this.applicationinfo.ApplicationName + ".exe", launchArgs);

                System.Console.WriteLine(info.Arguments);

                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.CreateNoWindow = true;
                info.FileName = "cmd.exe";
                Process run = Process.Start(info);
                run.WaitForExit();

                System.Console.WriteLine(this.applicationinfo.ApplicationName + " Updated - " + this.applicationinfo.ApplicationName + " has been updated to version " + myUpdateXml.Version);
            }
            catch(Exception e)
            {
                File.Delete(tempFilePath);
                File.Delete(currentPath + @"\update.zip");

                System.Console.WriteLine(this.applicationinfo.ApplicationName + " Updated - " + this.applicationinfo.ApplicationName + " Was not updated!\nMessage : " + e.Message);
                System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(e));
            }
        }

    }
}
