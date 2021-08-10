﻿// <copyright>
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
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.Win32;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.Utilities
{
    /// <summary>
    /// Helper class for collecting enviromental information about Rock install that is saved during update
    /// </summary>
    public static class RockUpdateHelper
    {
        private const int dotNet472ReleaseNumber = 461808;

        /// <summary>
        /// Returns the environment data as json.
        /// </summary>
        /// <returns>a JSON formatted string</returns>
        public static string GetEnvDataAsJson( System.Web.HttpRequest request, string rockUrl )
        {
            var envData = new Dictionary<string, string>();
            envData.Add( "AppRoot", rockUrl );
            envData.Add( "Architecture", ( IntPtr.Size == 4 ) ? "32bit" : "64bit" );
            envData.Add( "AspNetVersion", GetDotNetVersion() );
            envData.Add( "IisVersion", request.ServerVariables["SERVER_SOFTWARE"] );
            envData.Add( "ServerOs", Environment.OSVersion.ToString() );

            try
            { envData.Add( "SqlVersion", Rock.Data.DbService.ExecuteScaler( "SELECT SERVERPROPERTY('productversion')" ).ToString() ); }
            catch { }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var entityType = EntityTypeCache.Get( "Rock.Security.BackgroundCheck.ProtectMyMinistry", false, rockContext );
                    if ( entityType != null )
                    {
                        var pmmUserName = new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.Attribute.EntityTypeId.HasValue &&
                                v.Attribute.EntityTypeId.Value == entityType.Id &&
                                v.Attribute.Key == "UserName" )
                            .Select( v => v.Value )
                            .FirstOrDefault();
                        if ( !string.IsNullOrWhiteSpace( pmmUserName ) )
                        {
                            envData.Add( "PMMUserName", pmmUserName );
                        }
                    }
                }
            }
            catch { }

            return envData.ToJson();
        }

        /// <summary>
        /// Suggested approach to check which version of the .Net framework is installed when using version 4.5 or later
        /// as per https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx.
        /// </summary>
        /// <returns>a string containing the human readable version of the .Net framework</returns>
        public static DotNetVersionCheckResult CheckDotNetVersionFromRegistry()
        {
            // Check if Release is >= 461808 (4.7.2)
            if ( GetDotNetReleaseNumber() >= dotNet472ReleaseNumber )
            {
                return DotNetVersionCheckResult.Pass;
            }
            else
            {
                return DotNetVersionCheckResult.Fail;
            }
        }

        /// <summary>
        /// Gets the dot net release number from the registry.
        /// </summary>
        /// <returns></returns>
        public static int GetDotNetReleaseNumber()
        {
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
        }

        /// <summary>
        /// Gets the friendly string of the dot net version.
        /// </summary>
        /// <returns></returns>
        public static string GetDotNetVersion()
        {
            return GetDotNetVersion( GetDotNetReleaseNumber() );
        }

        /// <summary>
        /// Gets the dot net version.
        /// </summary>
        /// <param name="releaseNumber">The release number.</param>
        /// <returns></returns>
        public static string GetDotNetVersion( int releaseNumber )
        {
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
        }
    }
}
