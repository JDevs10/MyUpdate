using System;
using System.Net;
using System.Xml;

namespace MyUpdate
{
    /// <summary>
    /// Contains update information
    /// </summary>
    internal class MyUpdateXml
    {
        private Version version;
        private Uri uri;
        private string fileName;
        private string md5;
        private string description;
        private string launchArgs;

        internal Version Version
        {
            get { return this.version; }
        }
        internal Uri Uri
        {
            get { return this.uri; }
        }
        internal string FileName
        {
            get { return this.fileName; }
        }
        /// <summary>
        /// The MD5 of the update binary
        /// </summary>
        internal string MD5
        {
            get { return this.md5; }
        }
        internal string Description
        {
            get { return this.description; }
        }
        internal string LaunchArgs
        {
            get { return this.launchArgs; }
        }

        internal MyUpdateXml(Version version, Uri uri, string fileName, string md5, string description, string launchArgs)
        {
            this.version = version;
            this.uri = uri;
            this.fileName = fileName;
            this.md5 = md5;
            this.description = description;
            this.launchArgs = launchArgs;
        }

        /// <summary>
        /// Checks if update's version is newer than thee old version
        /// </summary>
        /// <param name="version">Application's current version</param>
        /// <returns>If the update's version # is newer</returns>
        internal bool IsNewerThan(Version version)
        {
            return this.version > version;
        }

        /// <summary>
        /// Checks the Uri to make sur file exist
        /// </summary>
        /// <param name="location">The uri of the update.xml</param>
        /// <returns>If the file exist</returns>
        internal static bool ExitsOnServer(Uri location)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(location.AbsoluteUri);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                resp.Close();

                return resp.StatusCode == HttpStatusCode.OK;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Parse the update.xml into MyUpdateXml object
        /// </summary>
        /// <param name="location">Uri of update.xml on the server</param>
        /// <param name="appId">The application's id</param>
        /// <returns>TThe MyUpdateXml object | null</returns>
        internal static MyUpdateXml Parse(Uri location, string appId)
        {
            Version version = null;
            string url = "", fileName = "", md5 = "", description = "", launchArgs = "";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(location.AbsoluteUri);

                XmlNode node = doc.DocumentElement.SelectSingleNode("//update[@appId='"+ appId + "']");

                if (node == null) { return null; }

                version = Version.Parse(node["version"].InnerText);
                url = node["url"].InnerText;
                fileName = node["fileName"].InnerText;
                md5 = node["md5"].InnerText;
                description = node["description"].InnerText;
                launchArgs = node["launchArgs"].InnerText;

                return new MyUpdateXml(version, new Uri(url), fileName, md5, description, launchArgs);
            }
            catch (Exception e){
                System.Console.WriteLine(e.Message);

                return null; 
            }
        }
    }
}
