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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The List block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "list" )]
    public class ListRenderer : StructuredContentBlockRenderer<ListData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, ListData data )
        {
            if ( data.Items.Count > 0 )
            {
                var tag = data.Style == ListData.OrderedStyle ? "ol" : "ul";

                writer.WriteLine( $"<{tag}>" );

                if ( data.Items[0] is string )
                {
                    foreach ( string item in data.Items )
                    {
                        writer.WriteLine( $"<li>{item}</li>" );
                    }
                }
                else
                {
                    RenderItemGroup( writer, tag, data.Items );
                }

                writer.WriteLine( $"</{tag}>" );
            }
        }

        private void RenderItemGroup( TextWriter writer, string tag, IEnumerable<dynamic> items )
        {
            foreach ( var item in items )
            {
                if ( ( ( ICollection ) item.items ).Count > 0 )
                {
                    writer.WriteLine( $"<li>{item.content}<{tag}>" );
                    RenderItemGroup( writer, tag, item.items );
                    writer.WriteLine( $"</{tag}></li>" );
                }
                else
                {
                    writer.WriteLine( $"<li>{item.content}</li>" );
                }
            }
        }
    }
}
