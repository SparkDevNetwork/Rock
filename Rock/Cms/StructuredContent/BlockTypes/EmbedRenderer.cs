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
using System.IO;
using System.Linq;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The Embed block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "embed" )]
    public class EmbedRenderer : StructuredContentBlockRenderer<EmbedData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, EmbedData data )
        {
            /*
                11/02/2026 - MSE

                The url comes from the stored structured content JSON, which an author can
                edit directly. Only http, https and relative urls are rendered, and the url
                is attribute encoded so it cannot break out of the src attribute.

                Reason: Prevent script urls and attribute injection in embed blocks.
            */
            var url = data.Service == "video" ? data.Source : data.Embed;

            if ( !IsAllowedUrl( url ) )
            {
                return;
            }

            if ( data.Service == "video" )
            {
                writer.Write( "<div class=\"embed-responsive embed-responsive-16by9\">" );
                writer.Write( $"<video class=\"embed-responsive-item\" src=\"{url.EncodeXml( true )}\" controls></video>" );
                writer.WriteLine( "</div>" );
            }
            else
            {
                writer.Write( "<div class=\"embed-responsive embed-responsive-16by9\">" );
                writer.Write( $"<iframe class=\"embed-responsive-item\" src=\"{url.EncodeXml( true )}\" allowfullscreen></iframe>" );
                writer.WriteLine( "</div>" );
            }
        }

        /// <summary>
        /// Determines whether the url can be safely written into an embed src attribute.
        /// Relative urls and absolute http or https urls are allowed. Anything else, such
        /// as javascript: or data: urls, or a url containing whitespace or control
        /// characters, is not.
        /// </summary>
        /// <param name="url">The url to check.</param>
        /// <returns><c>true</c> if the url is allowed; otherwise <c>false</c>.</returns>
        private static bool IsAllowedUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() || url.Any( c => char.IsControl( c ) || char.IsWhiteSpace( c ) ) )
            {
                return false;
            }

            // A browser only sees a scheme when a colon appears before the first
            // slash, question mark or hash. Without one the url is relative.
            var prefixLength = url.IndexOfAny( new[] { '/', '?', '#' } );
            var prefix = prefixLength < 0 ? url : url.Substring( 0, prefixLength );

            if ( !prefix.Contains( ':' ) )
            {
                return true;
            }

            return Uri.TryCreate( url, UriKind.Absolute, out var uri )
                && ( uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps );
        }
    }
}
