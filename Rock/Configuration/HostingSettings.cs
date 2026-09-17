// <copyright>
// Copyright by the Spark Development Network
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

using System;
using System.Collections.Generic;

using Microsoft.Win32;

namespace Rock.Configuration
{
    /// <inheritdoc cref="IHostingSettings" path="/summary"/>
    internal class HostingSettings : IHostingSettings
    {
        /// <inheritdoc/>
        public DateTime ApplicationStartDateTime { get; }

        /// <inheritdoc/>
        public string DotNetVersion => GetDotNetVersion();

        /// <inheritdoc/>
        public string WebRootPath { get; }

        /// <inheritdoc/>
        public string VirtualRootPath { get; }

        /// <inheritdoc/>
        public string MachineName { get; }

        /// <inheritdoc/>
        public string NodeName { get; }

        /// <inheritdoc/>
        public bool IsDevelopmentEnvironment { get; }

        public HostingSettings( IInitializationSettings initializationSettings )
        {
            ApplicationStartDateTime = RockDateTime.Now;
            MachineName = Environment.MachineName;

#if REVIEW_NET5_0_OR_GREATER
            WebRootPath = AppDomain.CurrentDomain.BaseDirectory;

            VirtualRootPath = "/";
#else
            WebRootPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath
                ?? AppDomain.CurrentDomain.BaseDirectory;

            VirtualRootPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath?.EnsureTrailingForwardslash()
                ?? "/";
#endif

#if REVIEW_WEBFORMS
            IsDevelopmentEnvironment = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;
#endif

            NodeName = initializationSettings.NodeName;

            if ( NodeName.IsNullOrWhiteSpace() )
            {
                NodeName = MachineName;
            }

            if ( NodeName.IsNullOrWhiteSpace() )
            {
                NodeName = Guid.NewGuid().ToString();
            }
        }

        #region Methods

        /// <summary>
        /// Gets the dot net release number from the registry.
        /// </summary>
        /// <returns></returns>
        public static int GetDotNetReleaseNumber()
        {
#if REVIEW_WEBFORMS
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using ( RegistryKey ndpKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ).OpenSubKey( subkey ) )
            {
                if ( ndpKey != null && ndpKey.GetValue( "Release" ) != null )
                {
                    return ( int ) ndpKey.GetValue( "Release" );
                }
                else
                {
                    return 0;
                }
            }
#else
            return 0;
#endif
        }

        /// <summary>
        /// Gets the friendly string of the dot net version.
        /// </summary>
        /// <returns></returns>
        internal static string GetDotNetVersion()
        {
#if NETFRAMEWORK
            return GetDotNetVersion( GetDotNetReleaseNumber() );
#else
            return Environment.Version.ToString();
#endif
        }

        /// <summary>
        /// Gets the dot net version.
        /// </summary>
        /// <param name="releaseNumber">The release number.</param>
        /// <returns></returns>
        internal static string GetDotNetVersion( int releaseNumber )
        {
#if NETFRAMEWORK
            var dotNetReleaseNumberVersionMap = new Dictionary<int, string>
            {
                { 528040, ".NET Framework 4.8" },
                { 461808, ".NET Framework 4.7.2" },
                { 461308, ".NET Framework 4.7.1" },
                { 460798, ".NET Framework 4.7" },
                { 394802, ".NET Framework 4.6.2" },
                { 394254, ".NET Framework 4.6.1" },
                { 393295, ".NET Framework 4.6" },
                { 379893, ".NET Framework 4.5.2" },
                { 378675, ".NET Framework 4.5.1" },
                { 378389, ".NET Framework 4.5" },
            };

            foreach ( var key in dotNetReleaseNumberVersionMap.Keys )
            {
                if ( releaseNumber >= key )
                {
                    return dotNetReleaseNumberVersionMap[key];
                }
            }

            return "Unknown";
#else
            return Environment.Version.ToString();
#endif
        }

        #endregion
    }
}
