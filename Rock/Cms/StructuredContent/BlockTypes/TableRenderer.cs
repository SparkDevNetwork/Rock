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
    /// The Table block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "table" )]
    public class TableRenderer : StructuredContentBlockRenderer<TableData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, TableData data )
        {
            bool isHeaderRow = data.WithHeadings, hasBody = false;

            writer.WriteLine( "<table class=\"table table-bordered table-striped\">" );

            foreach ( var row in data.Content )
            {
                string colTag = "td";

                // Determine if this the header row or the first body row.
                if ( isHeaderRow )
                {
                    writer.WriteLine( "<thead>" );
                    colTag = "th";
                }
                else if ( !hasBody )
                {
                    writer.WriteLine( "<tbody>" );
                    hasBody = true;
                }

                // Write the entire row contents.
                writer.Write( "<tr>" );
                foreach ( var col in row )
                {
                    writer.Write( $"<{colTag}>{col}</{colTag}>" );
                }
                writer.WriteLine( "</tr>" );

                // If this is the end of the header row, close it and prepare
                // for the body.
                if ( isHeaderRow )
                {
                    writer.WriteLine( "</thead>" );
                    isHeaderRow = false;
                }
            }

            if ( hasBody )
            {
                writer.WriteLine( "</tbody>" );
            }

            writer.WriteLine( "</table>" );
        }
    }
}
