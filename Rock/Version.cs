using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Rock
{
    /// <summary>
    /// 
    /// </summary>
    public static class Version
    {
        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static System.Version Current
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
    }
}