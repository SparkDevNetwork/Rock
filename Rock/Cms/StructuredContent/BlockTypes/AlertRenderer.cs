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
    /// The Bootstrap style alert block type used in the structured content system.
    /// </summary>
    [StructuredContentBlock( "alert" )]
    public class AlertRenderer : StructuredContentBlockRenderer<AlertData>
    {
        /// <inheritdoc/>
        protected override void Render( TextWriter writer, AlertData data )
        {
            var type = "info";
            var align = "left";

            if ( data.Type == "success" )
            {
                type = "success";
            }
            else if ( data.Type == "warning" )
            {
                type = "warning";
            }
            else if ( data.Type == "danger" )
            {
                type = "danger";
            }

            if ( data.Align == "center" )
            {
                align = "center";
            }
            else if ( data.Align == "right" )
            {
                align = "right";
            }

            writer.WriteLine( $"<div class=\"alert alert-{type} text-{align}\">{data?.Message}</div>" );
        }

        /// <inheritdoc/>
        protected override AlertData ResolveMergeFields( Dictionary<string, object> mergeFields, AlertData data )
        {
            if ( data != null )
            {
                data.Message = data.Message.ResolveMergeFields( mergeFields );
            }

            return data;
        }
    }
}
