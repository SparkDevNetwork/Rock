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
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Rock.Web
{
    /// <summary>
    /// Allows Static HTML Resources to have a Fingerprint so that we can set long cache expiration dates
    /// see http://madskristensen.net/post/cache-busting-in-aspnet
    /// </summary>
    public class Fingerprint
    {
        /// <summary>
        /// Tags the specified root relative path.
        /// </summary>
        /// <param name="rootRelativePath">The root relative path.</param>
        /// <returns></returns>
        public static string Tag( string rootRelativePath )
        {
            /*
                 ISSUE #4436 ⁃ Merge Fields Not Populating in Communication Wizard
                 Fixed the bug that happens when relaxedUrlToFileSystemMapping="false"
            */
            if ( rootRelativePath.Contains( "?" ) )
            {
                rootRelativePath = rootRelativePath.Remove( rootRelativePath.IndexOf( '?' ) );
            }

            if ( HttpRuntime.Cache[rootRelativePath] == null )
            {
                string absolute = HostingEnvironment.MapPath( rootRelativePath );
                if ( File.Exists( absolute ) )
                {
                    DateTime date = File.GetLastWriteTime( absolute );

                    string result = rootRelativePath + "?v=" + date.Ticks;
                    HttpRuntime.Cache.Insert( rootRelativePath, result, new CacheDependency( absolute ) );
                }
                else
                {
                    // If the file does not exist at the absolute path, log the failed attempt, and return the requested relative path.
                    Model.ExceptionLogService.LogException(
                        new Exception( string.Format( "Could not find the file at '{0}'.  Could not add fingerprint.", absolute ) ) );
                    return rootRelativePath;
                }
            }

            return HttpRuntime.Cache[rootRelativePath] as string;
        }
    }
}
