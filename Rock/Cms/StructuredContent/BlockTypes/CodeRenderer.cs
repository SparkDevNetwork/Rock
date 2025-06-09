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
    /// The Code block type used in the structured content system.
    /// </summary>
    /// <seealso cref="StructuredContentBlockRenderer{TData}" />
    [StructuredContentBlock( "code" )]
    public class CodeRenderer : StructuredContentBlockRenderer<CodeData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, CodeData data )
        {
            if ( data.Code.IsNotNullOrWhiteSpace() )
            {
                var language = data.Lang.IsNotNullOrWhiteSpace() ? $"language-{data.Lang}" : string.Empty;

                writer.WriteLine( $"<pre class=\"{language}\"><code class=\"language\">{data.Code.EncodeHtml()}</code></pre>" );
            }
        }

        /// <inheritdoc/>
        protected override CodeData ResolveMergeFields( Dictionary<string, object> mergeFields, CodeData data )
        {
            if ( data?.Code != null )
            {
                data.Code = data.Code.ResolveMergeFields( mergeFields );
            }

            return data;
        }
    }
}
