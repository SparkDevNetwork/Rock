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

        public static void HandleRequestLogging( HttpContext context )
        {
            // determine if we should log
            if ( ShouldLogRequest( context ) )
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