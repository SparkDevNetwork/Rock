//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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