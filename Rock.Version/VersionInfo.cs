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
        public static string GetRockProductVersion()
        {
            Assembly rockVersionDLL = Assembly.GetExecutingAssembly();
            FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockVersionDLL.Location );
            return rockDLLfvi.ProductVersion;
        }
    }
}