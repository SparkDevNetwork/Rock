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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Rock.Blocks
{
    /// <summary>
    /// Describes the result of a BlockAction in a platform agnostic way.
    /// </summary>
    public class BlockActionResult
    {
        #region Properties

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the CLR type of the content.
        /// </summary>
        /// <value>
        /// The CLR type of the content.
        /// </value>
        public Type ContentClrType { get; private set; }

        /// <summary>
        /// Gets the content data to be sent back in the response body.
        /// </summary>
        /// <value>
        /// The content to be sent back in the response body.
        /// </value>
        public object Content { get; set; }

        /// <summary>
        /// Gets the error message to be sent back.
        /// </summary>
        /// <value>
        /// The error message to be sent back.
        /// </value>
        public string Error { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public BlockActionResult( HttpStatusCode statusCode )
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        public BlockActionResult( HttpStatusCode statusCode, object content )
        {
            StatusCode = statusCode;
            Content = content;
            ContentClrType = content.GetType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        /// <param name="clrType">Type of the content.</param>
        public BlockActionResult( HttpStatusCode statusCode, object content, Type clrType )
        {
            StatusCode = statusCode;
            Content = content;
            ContentClrType = clrType;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the result and returns an object understood by the API controller.
        /// </summary>
        /// <param name="controller">The API controller handling the request.</param>
        /// <param name="defaultContentNegotiator">The content negotiator that will handle encoding JSON data.</param>
        /// <param name="validFormatters">The valid formatters for the negotiator.</param>
        /// <param name="cancellationToken">A token that specifies when the request was aborted.</param>
        /// <returns>A result that describes the HTTP response.</returns>
        internal virtual Task<IHttpActionResult> ExecuteAsync( ApiController controller, IContentNegotiator defaultContentNegotiator, List<MediaTypeFormatter> validFormatters, CancellationToken cancellationToken )
        {
            var isErrorStatusCode = ( int ) StatusCode >= 400;

            if ( isErrorStatusCode && Content is string )
            {
                return Task.FromResult<IHttpActionResult>( new NegotiatedContentResult<HttpError>( StatusCode, new HttpError( Content.ToString() ), defaultContentNegotiator, controller.Request, validFormatters ) );
            }
            else if ( Error != null )
            {
                return Task.FromResult<IHttpActionResult>( new NegotiatedContentResult<HttpError>( StatusCode, new HttpError( Error ), defaultContentNegotiator, controller.Request, validFormatters ) );
            }
            else if ( Content is HttpContent httpContent )
            {
                var response = controller.Request.CreateResponse( StatusCode );
                response.Content = httpContent;
                return Task.FromResult<IHttpActionResult>( new ResponseMessageResult( response ) );
            }
            else if ( Content is Stream stream )
            {
                if ( !( stream is MemoryStream ms ) || !ms.TryGetBuffer( out var buffer ) )
                {
                    buffer = new ArraySegment<byte>( stream.ReadBytesToEnd() );
                }

                var response = new HttpResponseMessage( HttpStatusCode.OK )
                {
                    Content = new ByteArrayContent( buffer.Array )
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/octet-stream" );

                return Task.FromResult<IHttpActionResult>( new ResponseMessageResult( response ) );
            }
            else if ( ContentClrType != null )
            {
                var genericType = typeof( NegotiatedContentResult<> ).MakeGenericType( ContentClrType );
                return Task.FromResult( ( IHttpActionResult ) Activator.CreateInstance( genericType, StatusCode, Content, controller ) );
            }
            else
            {
                return Task.FromResult<IHttpActionResult>( new StatusCodeResult( StatusCode, controller ) );
            }
        }

        #endregion
    }
}
