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
    /// The Paragraph block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "checklist" )]
    public class ChecklistRenderer : StructuredContentBlockRenderer<ChecklistData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, ChecklistData data )
        {
            if ( data.Items != null && data.Items.Count > 0 )
            {
                writer.WriteLine( "<p>" );

                for ( int i = 0; i < data.Items.Count; i++ )
                {
                    var item = data.Items[i];

                    if ( item.Checked )
                    {
                        writer.Write( "<i class=\"fa fa-check-square\"></i> " );
                    }
                    else
                    {
                        writer.Write( "<i class=\"fa fa-square-o\"></i> " );
                    }

                    writer.Write( item.Text );

                    if ( i + 1 < data.Items.Count )
                    {
                        writer.Write( "<br>" );
                    }
                }

                writer.WriteLine( "</p>" );
            }
        }
    }
}
