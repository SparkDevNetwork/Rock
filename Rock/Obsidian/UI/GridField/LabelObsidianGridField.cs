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
using System.Web;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Value-shaping subclass that renders a value as a Rock-styled label (badge)
    /// via server-rendered HTML. Backs the DefinedValueField behavior of
    /// attribute-typed DataSelects.
    /// </summary>
    public class LabelObsidianGridField : HtmlObsidianGridField
    {
        /// <summary>
        /// Rock label CSS suffix (e.g. <c>info</c>, <c>success</c>, <c>warning</c>,
        /// <c>danger</c>, <c>default</c>). Consumers set this to control the badge
        /// color.
        /// </summary>
        public string LabelType { get; set; } = "default";

        /// <inheritdoc/>
        public override object TransformValue( object rawValue, ObsidianGridFieldContext context )
        {
            if ( rawValue == null )
            {
                return string.Empty;
            }

            var text = rawValue.ToString();
            if ( string.IsNullOrEmpty( text ) )
            {
                return string.Empty;
            }

            return $"<span class=\"label label-{LabelType}\">{HttpUtility.HtmlEncode( text )}</span>";
        }
    }
}
