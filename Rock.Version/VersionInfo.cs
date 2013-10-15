//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Diagnostics;
using System.Reflection;

namespace Rock.VersionInfo
{
    /// <summary>
    /// Returns current information about Rock
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>
        /// Gets the rock product version.
        /// </summary>
        /// <returns></returns>
        public static string GetRockProductVersion()
        {
            Assembly rockVersionDLL = Assembly.GetExecutingAssembly();
            FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockVersionDLL.Location );
            return rockDLLfvi.ProductVersion;
        }

        /// <summary>
        /// Gets the rock product name.
        /// </summary>
        /// <returns></returns>
        public static string GetRockProductName()
        {
            return "Rock ChMS";
        }
    }
}