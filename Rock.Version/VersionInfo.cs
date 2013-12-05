//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Diagnostics;
using System.Reflection;

namespace Rock.VersionInfo
{
    public static class VersionInfo
    {
        /// <summary>
        /// Provides the full 'official' product name of the Rock version currently in use.
        /// </summary>
        /// <returns>the full name of this Rock version (ex. "Rock McKinley 1.2.5" or "Rock Humphreys 0.1.0 (alpha)"</returns>
        public static string GetRockProductVersionFullName()
        {
            return GetFileVersionInfo().ProductVersion;
        }

        /// <summary>
        /// Provides the full product version number of the Rock currently in use.
        /// </summary>
        /// <returns>the full Rock version number (ex. 1.0.0.3)</returns>
        public static string GetRockProductVersionNumber()
        {
            return GetFileVersionInfo().FileVersion;
        }

        private static FileVersionInfo GetFileVersionInfo()
        {
            Assembly rockVersionDLL = Assembly.GetExecutingAssembly();
            FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockVersionDLL.Location );
            return rockDLLfvi;
        }
    }
}