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
using System.IO;
using System.Net.Http.Headers;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// The output of a merge template that is sent straight to the browser
    /// instead of being saved as a <see cref="Rock.Model.BinaryFile"/>.
    /// </summary>
    [Rock.Attribute.RockInternal( "17.12" )]
    public sealed class MergeTemplateDocument
    {
        #region Fields

        /// <summary>
        /// The file name used when the merge template type does not provide one.
        /// </summary>
        private const string DefaultFileName = "MergeTemplateOutput";

        /// <summary>
        /// The MIME type used when the merge template type does not provide a usable one.
        /// </summary>
        private const string DefaultMimeType = "application/octet-stream";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the stream that contains the merged document. When the stream
        /// supports seeking it is positioned at the start.
        /// </summary>
        /// <value>The stream that contains the merged document.</value>
        public Stream ContentStream { get; }

        /// <summary>
        /// Gets the file name the browser should save the document as.
        /// </summary>
        /// <value>The file name of the document.</value>
        public string FileName { get; }

        /// <summary>
        /// Gets the MIME type of the document, without any parameters.
        /// </summary>
        /// <value>The MIME type of the document.</value>
        public string MimeType { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeTemplateDocument"/> class.
        /// </summary>
        /// <param name="contentStream">The stream that contains the merged document.</param>
        /// <param name="fileName">The file name the browser should save the document as. A blank value uses "MergeTemplateOutput".</param>
        /// <param name="mimeType">The MIME type of the document. A blank or invalid value uses "application/octet-stream".</param>
        public MergeTemplateDocument( Stream contentStream, string fileName, string mimeType )
        {
            if ( contentStream != null && contentStream.CanSeek )
            {
                contentStream.Seek( 0, SeekOrigin.Begin );
            }

            ContentStream = contentStream;
            FileName = fileName.IsNullOrWhiteSpace() ? DefaultFileName : fileName.Trim();
            MimeType = GetNormalizedMimeType( mimeType );
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the MIME type without any parameters, or the default MIME
        /// type when the value is blank or not a valid media type.
        /// </summary>
        /// <param name="mimeType">The MIME type to normalize.</param>
        /// <returns>A MIME type that can be sent in a Content-Type header.</returns>
        private static string GetNormalizedMimeType( string mimeType )
        {
            /*
                10/21/2026 - MSE

                The MIME type comes from the uploaded template file, so it can be blank or
                carry parameters such as "text/html; charset=utf-8". The Obsidian block
                actions send this value through MediaTypeHeaderValue, whose constructor
                throws for anything that is not a bare "type/subtype". Strip parameters and
                fall back to a generic type so a valid merge always downloads.

                Reason: Prevent a malformed template MIME type from breaking the download.
            */
            if ( mimeType.IsNullOrWhiteSpace() )
            {
                return DefaultMimeType;
            }

            var separatorIndex = mimeType.IndexOf( ';' );
            var mediaType = ( separatorIndex >= 0 ? mimeType.Substring( 0, separatorIndex ) : mimeType ).Trim();

            if ( mediaType.IsNullOrWhiteSpace() || !MediaTypeHeaderValue.TryParse( mediaType, out _ ) )
            {
                return DefaultMimeType;
            }

            return mediaType;
        }

        #endregion Methods
    }
}
