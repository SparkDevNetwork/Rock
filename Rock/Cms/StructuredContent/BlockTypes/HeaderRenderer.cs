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
using System.Collections.Generic;
using System.IO;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The Header block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "header" )]
    public class HeaderRenderer : StructuredContentBlockRenderer<HeaderData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, HeaderData data )
        {
            var level = Math.Min( 6, Math.Max( 1, data.Level ) );

            writer.WriteLine( $"<h{level}>{data?.Text}</h{level}>" );
        }

        /// <inheritdoc/>
        protected override HeaderData ResolveMergeFields( Dictionary<string, object> mergeFields, HeaderData data )
        {
            if ( data != null )
            {
                data.Text = data.Text.ResolveMergeFields( mergeFields );
            }

            return data;
        }
    }
}
