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

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Rock.Blocks
{
    /// <summary>
    /// Provides a way to stream data to the client using Server-Sent Events (SSE).
    /// </summary>
    /// <typeparam name="T">The type of each item to be converted to JSON and streamed.</typeparam>
    internal class ServerSentEventsBlockActionResult<T> : BlockActionResult
    {
        #region Fields

        /// <summary>
        /// The source of items that will be streamed to the client.
        /// </summary>
        private readonly IAsyncEnumerable<T> _source;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ServerSentEventsBlockActionResult{T}"/> class.
        /// </summary>
        /// <param name="source">The source of items that will be streamed to the client.</param>
        public ServerSentEventsBlockActionResult( IAsyncEnumerable<T> source )
            : base( System.Net.HttpStatusCode.OK )
        {
            _source = source;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override Task<IHttpActionResult> ExecuteAsync( ApiController controller, IContentNegotiator defaultContentNegotiator, List<MediaTypeFormatter> validFormatters, CancellationToken cancellationToken )
        {
            var formatter = defaultContentNegotiator.Negotiate( typeof( T ), controller.Request, validFormatters ).Formatter;

            var response = new HttpResponseMessage( StatusCode )
            {
                Content = new PushStreamContent( async ( stream, content, context ) =>
                {
                    using var writer = new System.IO.StreamWriter( stream );

                    await foreach ( var item in _source )
                    {
                        await writer.WriteAsync( "data: " );
                        await writer.FlushAsync();

                        await formatter.WriteToStreamAsync( typeof( T ), item, stream, content, context, cancellationToken );

                        await writer.WriteAsync( "\n\n" );
                        await writer.FlushAsync();
                    }

                    await writer.WriteAsync( "data: [DONE]\n" );
                    await writer.FlushAsync();
                }, "text/event-stream" )
            };

            response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            return Task.FromResult( ( IHttpActionResult ) new System.Web.Http.Results.ResponseMessageResult( response ) );
        }

        #endregion
    }
}
