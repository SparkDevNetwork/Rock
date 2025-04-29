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

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The Quote block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "quote" )]
    public class QuoteRenderer : StructuredContentBlockRenderer<QuoteData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, QuoteData data )
        {
            if ( data.Text.IsNotNullOrWhiteSpace() )
            {
                string alignment = string.Empty;

                if ( data.Alignment == QuoteData.CenterAlignment )
                {
                    alignment = " class=\"text-center\"";
                }

                if ( data.Caption.IsNotNullOrWhiteSpace() )
                {
                    writer.WriteLine( "<figure>" );
                }

                writer.WriteLine( "<blockquote>" );
                writer.WriteLine( $"<p{alignment}>{data.Text}</p>" );
                writer.WriteLine( "</blockquote>" );

                if ( data.Caption.IsNotNullOrWhiteSpace() )
                {
                    writer.WriteLine( $"<figcaption>{data.Caption}</figcaption></figure>" );
                }
            }
        }

        /// <inheritdoc/>
        protected override QuoteData ResolveMergeFields( Dictionary<string, object> mergeFields, QuoteData data )
        {
            if ( data != null )
            {
                data.Text = data.Text.ResolveMergeFields( mergeFields );
                data.Caption = data.Caption.ResolveMergeFields( mergeFields );
            }

            return data;
        }
    }
}
