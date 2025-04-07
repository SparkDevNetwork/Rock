using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.AI.Classes.ChatCompletions;

namespace Rock.Rest.Controllers
{
    [Rock.SystemGuid.RestControllerGuid( "B723EAC9-8B4E-2392-4985-31AA335B7E45" )]
    /// <summary>
    /// Ai Controller
    /// </summary>
    public class AiController : ApiControllerBase {

        [HttpGet]
        [System.Web.Http.Route( "api/Ai/StreamCompletion" )]
        [Rock.SystemGuid.RestActionGuid( "6F8FBFFC-A100-51B1-4A54-82A5B91A6846" )]
        public HttpResponseMessage StreamCompletion( [FromUri] string prompt )
        {
            var response = Request.CreateResponse( HttpStatusCode.OK );
            response.Content = new PushStreamContent( async ( stream, content, context ) =>
            {
                // Create the request
                var chatCompletionsRequest = new ChatCompletionsRequest();
                chatCompletionsRequest.Messages.Add( new ChatCompletionsRequestMessage() { Role = Rock.Enums.AI.ChatMessageRole.User, Content = prompt.Trim() } );
                chatCompletionsRequest.Model = "gpt-4o-mini"; // TODO: Set the model based on your requirements
                // TODO: also set the model and other properties as needed

                // Get the AI provider
                var aiProvider = GetProvider( null, new RockContext() ); // TODO: Pass the providerComponentId if needed

                if ( aiProvider == null )
                {
                    throw new HttpResponseException( Request.CreateErrorResponse( HttpStatusCode.NotFound, "No active AI provider found." ) );
                }

                var aiComponent = aiProvider.GetAIComponent(); 
                await aiComponent.StreamChatCompletions( aiProvider, chatCompletionsRequest, stream );

                // Ensure the last chunk is written before closing the stream
                await stream.FlushAsync();
                stream.Close();
            }, "text/event-stream" );

            return response;
        }


        #region Private Methods

        /// <summary>
        /// Gets the active AI provider based on the provided component ID or name. This code is duplicated from
        /// the AI Completions short code 
        /// </summary>
        /// <param name="providerComponentId"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private AIProvider GetProvider( string providerComponentId, RockContext rockContext )
        {
            AIProvider provider;

            // Get the active AI provider
            var providerService = new AIProviderService( rockContext );

            if ( providerComponentId.IsNullOrWhiteSpace() )
            {
                // Use the first active provider.
                provider = providerService.GetActiveProvider();
            }
            else
            {
                // Get the provider by Guid or ID...
                provider = providerService.Get( providerComponentId, allowIntegerIdentifier: true );

                if ( provider == null )
                {
                    // ...or try to match by name.
                    provider = providerService.Queryable().FirstOrDefault( p => p.Name.Equals( providerComponentId, StringComparison.OrdinalIgnoreCase ) );
                }
            }

            return provider;
        }
        #endregion
    }


}
