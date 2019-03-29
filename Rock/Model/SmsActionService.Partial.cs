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
using System.Linq;
using System.Web;
using Rock.Communication.SmsActions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SmsActionService
    {
        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The message received by the communications component.</param>
        /// <returns>If not null, identifies the response that should be sent back to the sender.</returns>
        static public SmsMessage ProcessIncomingMessage( SmsMessage message )
        {
            SmsMessage response = null;
            var errorMessage = string.Empty;

            var smsActions = SmsActionCache.All()
                .Where( a => a.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id );

            foreach ( var smsAction in smsActions )
            {
                if ( smsAction.SmsActionComponent == null )
                {
                    LogIfError( string.Format( "The SmsActionComponent for {0} was null", smsAction.Name ) );
                    continue;
                }

                try
                {
                    //
                    // Check if the action wants to process this message.
                    //
                    var shouldProcessMessage = smsAction.SmsActionComponent.ShouldProcessMessage( smsAction, message, out errorMessage );
                    LogIfError( errorMessage );

                    if ( !shouldProcessMessage )
                    {
                        continue;
                    }

                    //
                    // Process the message and use either the response returned by the action
                    // or the previous response we already had.
                    //
                    response = smsAction.SmsActionComponent.ProcessMessage( smsAction, message, out errorMessage ) ?? response;
                    LogIfError( errorMessage );

                    //
                    // If the action is set to not continue after processing then stop.
                    //
                    if ( !smsAction.ContinueAfterProcessing )
                    {
                        return response;
                    }
                }
                catch ( Exception exception )
                {
                    LogIfError( exception );
                }
            }

            return response;
        }

        /// <summary>
        /// If the exception is not null, log it
        /// </summary>
        /// <param name="exception"></param>
        static private void LogIfError( Exception exception )
        {
            if ( exception == null )
            {
                return;
            }

            var context = HttpContext.Current;
            var wrappedException = new Exception( "An exception occured in the SmsAction pipeline. See the inner exception.", exception );
            ExceptionLogService.LogException( wrappedException, context );
        }

        /// <summary>
        /// If the errorMessage is not empty, log it
        /// </summary>
        /// <param name="errorMessage"></param>
        static private void LogIfError( string errorMessage )
        {
            if ( string.IsNullOrEmpty( errorMessage ) )
            {
                return;
            }

            var context = HttpContext.Current;
            var exception = new Exception( string.Format( "An error occured in the SmsAction pipeline: {0}", errorMessage ) );
            ExceptionLogService.LogException( exception, context );
        }
    }
}
