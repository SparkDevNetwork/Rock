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

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The Attachment block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "attachment" )]
    public class AttachmentRenderer : StructuredContentBlockRenderer<AttachmentData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, AttachmentData data )
        {
            var title = data.Title ?? string.Empty;
            var url = data.File?.Url ?? string.Empty;
            var extension = data.File?.Name != null ? Path.GetExtension( data.File.Name ) : string.Empty;

            if ( extension.StartsWith( "." ) )
            {
                extension = extension.Substring( 1 ).ToLower();
            }
            else
            {
                extension = extension.ToLower();
            }

            writer.Write( "<div class=\"structuredcontent-attachment\">" );

            writer.Write( "<div class=\"structuredcontent-attachment-fileicon\">" );
            writer.Write( "<div class=\"structuredcontent-attachment-fileicon-background\"></div>" );
            writer.Write( $"<div class=\"structuredcontent-attachment-fileicon-label\" title=\"{extension.EncodeXml( true )}\">{extension.EncodeXml()}</div>" );
            writer.Write( "</div>" );

            writer.Write( "<div class=\"structuredcontent-attachment-fileinfo\">" );
            writer.Write( $"<div class=\"structuredcontent-attachment-fileinfo-title\">{title.EncodeXml()}</div>" );
            WriteSize( writer, data );
            writer.Write( "</div>" );

            writer.Write( $"<a class=\"structuredcontent-attachment-download\" href=\"{url.EncodeXml( true )}\" target=\"_blank\" rel=\"nofollow noindex noreferer\">" );
            writer.Write( "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"icon icon-tabler icons-tabler-outline icon-tabler-download\"><path d=\"M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2 -2v-2\"></path><path d=\"M7 11l5 5l5 -5\"></path><path d=\"M12 4l0 12\"></path></svg>" );
            writer.Write( "</a>" );

            writer.Write( "</div>" );
        }

        /// <summary>
        /// Writes the file size element if there is a valid size.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The block data.</param>
        private static void WriteSize( TextWriter writer, AttachmentData data )
        {
            if ( data.File == null || data.File.Size <= 0 )
            {
                return;
            }

            string sizeSuffix;
            double size = data.File.Size;

            if ( Math.Log10( size ) >= 6 )
            {
                sizeSuffix = "MiB";
                size /= Math.Pow( 2, 20 );
            }
            else
            {
                sizeSuffix = "KiB";
                size /= Math.Pow( 2, 10 );
            }

            writer.Write( $"<div class=\"structuredcontent-attachment-fileinfo-size\" data-size=\"{sizeSuffix}\">{size:F1}</div>" );
        }
    }
}
