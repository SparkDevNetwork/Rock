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
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock;

namespace RockWeb
{
    /// <summary>
    /// Summary description for TwilioLogging
    /// </summary>
    public static class TwilioLogging
    {
        private static bool ShouldLogRequest( HttpContext context )
        {
            return ( !string.IsNullOrWhiteSpace( context.Request.QueryString["Log"] ) && context.Request.QueryString["Log"] == "true" );
        }

        public static void HandleRequestLogging( HttpContext context, bool alwaysLog )
        {
            // determine if we should log
            if ( alwaysLog || ShouldLogRequest( context ) )
            {
                LogRequest( context );
            }
        }

        private static void LogRequest( HttpContext context )
        {
            var request = context.Request;
            var formValues = new List<string>();
            formValues.Add( string.Format( "{0}: '{1}'", "End Point URL", request.Url ) );
            foreach ( string name in request.Form.AllKeys )
            {
                formValues.Add( string.Format( "{0}: '{1}'", name, request.Form[name] ) );
            }

            WriteRequestToTwilioLog( context, formValues.AsDelimited( ", " ) );
        }

        private static void WriteRequestToTwilioLog( HttpContext context, string message )
        {
            string logFile = context.Server.MapPath( "~/App_Data/Logs/TwilioLog.txt" );

            // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
            var maxRetry = 3;
            for ( int retry = 0; retry < maxRetry; retry++ )
            {
                try
                {
                    using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
                    {
                        using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                        {
                            sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
                            break;
                        }
                    }
                }
                catch ( System.IO.IOException )
                {
                    if ( retry < maxRetry - 1 )
                    {
                        System.Threading.Tasks.Task.Delay( 2000 ).Wait();
                    }
                }
            }

        }
    }
}