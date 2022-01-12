﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace MyUpdate
{
    internal partial class MyUpdateDownloadForm : Form
    {
        private WebClient webClient;
        private BackgroundWorker bgWorker;
        private string tempFile;
        private string md5;
        internal string TempFilePath
        {
            get { return this.tempFile; }
        }


        internal MyUpdateDownloadForm(Uri location, string md5, Icon progressIcon)
        {
            InitializeComponent();

            if(progressIcon != null)
            {
                this.Icon = progressIcon;
            }

            tempFile = Path.GetTempFileName();
            this.md5 = md5;

            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);

            try
            {
                webClient.DownloadFileAsync(location, this.tempFile);
            }
            catch (Exception e){
                this.DialogResult = DialogResult.No; 
                this.Close(); 
            }
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.progressBar.Value = e.ProgressPercentage;
            this.lblProgress.Text = String.Format("Download {0} of {1}", FormatByBytes(e.BytesReceived, 1, true), FormatByBytes(e.TotalBytesToReceive, 1, true));
        }

        private string FormatByBytes(long bytes, int decimalPlaces, bool showByteType)
        {
            double newBytes = bytes;
            string formatString = "{0";
            string byteType = "B";

            if(newBytes > 1024 && newBytes < 1048576)
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

            if(decimalPlaces > 0)
            {
                formatString += ":0.";
            }

            for (int i = 0; i<decimalPlaces; i++) { }
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
            if(e.Error != null)
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            }
            else if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            else
            {
                lblProgress.Text = "Verifying Download...";
                progressBar.Style = ProgressBarStyle.Marquee;

                bgWorker.RunWorkerAsync(new string[] { this.tempFile, this.md5 });
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string file = ((string[])e.Argument)[0];
            string filemd5 = Hasher.HashFile(file, HashType.MD5);
            string updatemd5 = ((string[])e.Argument)[1].ToLower();

            System.Console.WriteLine("file : " + Newtonsoft.Json.JsonConvert.SerializeObject(e.Argument));
            System.Console.WriteLine("file : "+ file);
            System.Console.WriteLine("filemd5 : " + filemd5);
            System.Console.WriteLine("HashType : " + HashType.MD5);
            System.Console.WriteLine("updatemd5 : " + updatemd5);

            if (filemd5 != updatemd5)
            {
                e.Result = DialogResult.No;
            }
            else
            {
                e.Result = DialogResult.OK;
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = (DialogResult)e.Result;
            this.Close();
        }

        private void MyUpdateDownloadForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
                this.DialogResult = DialogResult.Abort;
            }

            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                this.DialogResult = DialogResult.Abort;
            }
        }
    }
    
}
