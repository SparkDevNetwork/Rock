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
            if ( data.Service == "video" )
            {
                writer.Write( "<div class=\"embed-responsive embed-responsive-16by9\">" );
                writer.Write( $"<video class=\"embed-responsive-item\" src=\"{data.Source}\" controls></video>" );
                writer.WriteLine( "</div>" );
            }
            else
            {
                writer.Write( "<div class=\"embed-responsive embed-responsive-16by9\">" );
                writer.Write( $"<iframe class=\"embed-responsive-item\" src=\"{data.Embed}\" allowfullscreen></iframe>" );
                writer.WriteLine( "</div>" );
            }
        }
    }
}
