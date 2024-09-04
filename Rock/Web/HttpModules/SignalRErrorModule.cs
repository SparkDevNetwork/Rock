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
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace Rock.Web.HttpModules
{
    /// <summary>
    /// Handles errors from the SignalR pipeline.
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.SignalR.Hubs.HubPipelineModule" />
    public class SignalRErrorModule : HubPipelineModule
    {
        /// <inheritdoc/>
        protected override void OnIncomingError( ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext )
        {
            if ( exceptionContext?.Error is HttpException httpEx )
            {
                // Check for client/remote host disconnection error and ignore as it
                // indicates the server it trying to write a response to a disconnected client.
                if( httpEx.Message.IsNotNullOrWhiteSpace() && httpEx.StackTrace.IsNotNullOrWhiteSpace() &&
                    httpEx.Message.Contains( "The remote host closed the connection." ) &&
                    httpEx.StackTrace.Contains( "Microsoft.AspNet.SignalR.Owin.ServerResponse.Write" ) )
                {
                    // Clear the exception so it doesn't get logged.
                    exceptionContext.Error = null;
                    return;
                }
            }
        }
    }
}

