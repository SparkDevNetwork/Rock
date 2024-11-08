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
    /// Provides file download and raw data transmission support for block
    /// actions.
    /// </summary>
    internal class FileBlockActionResult : BlockActionResult
    {
        #region Fields

        /// <summary>
        /// The stream containing the data.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// The MIME content type.
        /// </summary>
        private readonly string _contentType;

        /// <summary>
        /// The suggested filename for download.
        /// </summary>
        private readonly string _filename;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new result for that will result in a file download for
        /// the data in the stream.
        /// </summary>
        /// <param name="stream">The stream that contains the data to send. This will be automatically disposed after the response is sent.</param>
        /// <param name="contentType">The MIME content type for the data.</param>
        public FileBlockActionResult( Stream stream, string contentType )
            : this( stream, contentType, null )
        {
        }

        /// <summary>
        /// Creates a new result for that will result in a file download for
        /// the data in the stream.
        /// </summary>
        /// <param name="stream">The stream that contains the data to send. This will be automatically disposed after the response is sent.</param>
        /// <param name="contentType">The MIME content type for the data.</param>
        /// <param name="filename">If specified this will provide a default filename and cause the browser to download the data.</param>
        public FileBlockActionResult( Stream stream, string contentType, string filename )
            : base( HttpStatusCode.OK )
        {
            _stream = stream;
            _contentType = contentType;
            _filename = filename;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        internal override Task<IHttpActionResult> ExecuteAsync( ApiController controller, IContentNegotiator defaultContentNegotiator, List<MediaTypeFormatter> validFormatters, CancellationToken cancellationToken )
        {
            var response = new HttpResponseMessage( HttpStatusCode.OK )
            {
                Content = new ByteArrayContent( _stream.ReadBytesToEnd() )
            };

            if ( _stream.CanSeek )
            {
                response.Content.Headers.ContentLength = _stream.Length;
            }

            if ( _filename.IsNotNullOrWhiteSpace() )
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue( "attachment" )
                {
                    FileName = _filename
                };
            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue( _contentType );

            return Task.FromResult<IHttpActionResult>( new ResponseMessageResult( response ) );
        }

        #endregion
    }
}
