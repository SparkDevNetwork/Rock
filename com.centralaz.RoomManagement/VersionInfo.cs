// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Diagnostics;
using System.Reflection;

namespace com.centralaz.RoomManagement
{
    public static class VersionInfo
    {
        /// <summary>
        /// Provides the full 'official' product name of the plugin (assembly) version currently in use.
        /// </summary>
        /// <returns>the full name of this plugin version (ex. "Blah 1.2.5" or "Blah 0.1.0 (alpha))"</returns>
        public static string GetPluginVersionFullName()
        {
            return GetFileVersionInfo().ProductVersion;
        }

        /// <summary>
        /// Provides the full product version number of the plugin currently in use.
        /// </summary>
        /// <returns>the full plugin version number (ex. 1.0.0.3)</returns>
        public static string GetPluginProductVersionNumber()
        {
            return GetFileVersionInfo().FileVersion;
        }

        /// <summary>
        /// Provides the semantic version number of the plugin currently in use.
        /// </summary>
        /// <returns>the semantic plugin version number (ex. 1.0.0)</returns>
        public static string GetPluginSemanticVersionNumber()
        {
            return string.Format( "{0}.{1}.{2}", GetFileVersionInfo().FileMajorPart.ToString(), GetFileVersionInfo().FileMinorPart.ToString(), GetFileVersionInfo().FileBuildPart.ToString() );
        }

        private static FileVersionInfo GetFileVersionInfo()
        {
            Assembly pluginVersionDLL = Assembly.GetExecutingAssembly();
            FileVersionInfo pluginDLLfvi = FileVersionInfo.GetVersionInfo( pluginVersionDLL.Location );
            return pluginDLLfvi;
        }
    }
}