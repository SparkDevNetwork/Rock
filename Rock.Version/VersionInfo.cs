// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

        /// <summary>
        /// Provides the semantic version number of the Rock currently in use.
        /// </summary>
        /// <returns>the semantic Rock version number (ex. 1.0.0)</returns>
        public static string GetRockSemanticVersionNumber()
        {
            return string.Format( "{0}.{1}.{2}", GetFileVersionInfo().FileMajorPart.ToString(), GetFileVersionInfo().FileMinorPart.ToString(), GetFileVersionInfo().FileBuildPart.ToString() );
        }

        private static FileVersionInfo GetFileVersionInfo()
        {
            Assembly rockVersionDLL = Assembly.GetExecutingAssembly();
            FileVersionInfo rockDLLfvi = FileVersionInfo.GetVersionInfo( rockVersionDLL.Location );
            return rockDLLfvi;
        }
    }
}