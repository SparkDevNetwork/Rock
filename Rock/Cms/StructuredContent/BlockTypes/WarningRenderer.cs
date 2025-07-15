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
    /// The Warning block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "warning" )]
    public class WarningRenderer : StructuredContentBlockRenderer<WarningData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, WarningData data )
        {
            if ( data.Message.IsNullOrWhiteSpace() )
            {
                return;
            }

            writer.WriteLine( "<div class=\"alert alert-warning\">" );

            if ( data.Title.IsNotNullOrWhiteSpace() )
            {
                writer.WriteLine( $"<strong>{data.Title}</strong> " );
            }

            writer.WriteLine( data.Message );

            writer.WriteLine( $"</div>" );
        }

        /// <inheritdoc/>
        protected override WarningData ResolveMergeFields( Dictionary<string, object> mergeFields, WarningData data )
        {
            if ( data != null )
            {
                data.Message = data.Message.ResolveMergeFields( mergeFields );
                data.Title = data.Title.ResolveMergeFields( mergeFields );
            }

            return data;
        }
    }
}
