using System;
using System.Drawing;
using System.Reflection;

namespace MyUpdate.Console
{
    /// <summary>
    /// The interface that all apliccations need to implement in order to user MyUpdate
    /// </summary>
    public interface IMyUpdatable
    {
        /// <summary>
        /// The name of your appliccation as you it displayed on the update form
        /// </summary>
        string ApplicationName { get; }
        /// <summary>
        /// An identifier string to use to identify your application in the update.xml
        /// Should be the same as your appId in the update.xml
        /// </summary>
        string ApplicationID { get; }
        /// <summary>
        /// The current assembly
        /// </summary>
        Assembly ApplicationAssembly { get; }
        /// <summary>
        /// The location of the update.xml on the server
        /// </summary>
        Uri UpdateXmlLocation { get; }
    }
}
