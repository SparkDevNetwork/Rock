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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rock.Utility
{
    /// <summary>
    /// Helper Class to do a couple of handy things from a HttpContext
    /// </summary>
    public static class WebRequestHelper
    {
        /// <summary>
        /// Get the host name from request
        /// </summary>
        /// <param name="context">The HTTP Context.</param>
        /// <returns></returns>
        public static string GetHostNameFromRequest( HttpContext context )
        {
            var forwardedHost = context.Request.ServerVariables["HTTP_X_Forwarded_Host"];
            if ( forwardedHost.IsNotNullOrWhiteSpace() )
            {
                return forwardedHost;
            }
            return context.Request.Url.Host;
        }

        /// <summary>
        /// Get if connection is secure
        /// </summary>
        /// <param name="context">The HTTP Context.</param>
        /// <returns></returns>
        public static bool IsSecureConnection( HttpContext context )
        {
            return String.Equals( context.Request.ServerVariables["HTTP_X_FORWARDED_PROTO"], "https", StringComparison.OrdinalIgnoreCase ) || context.Request.IsSecureConnection;
        }
    }
}
