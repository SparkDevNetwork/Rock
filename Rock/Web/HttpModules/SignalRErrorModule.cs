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

