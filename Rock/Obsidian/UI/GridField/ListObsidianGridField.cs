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
using System.Linq;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Value-shaping subclass that joins an <see cref="IEnumerable"/> value with a
    /// configurable delimiter. Backs the ListDelimitedField and list-shaped
    /// CallbackField DataSelects (parent names, related people, group
    /// participation, etc.).
    /// </summary>
    public class ListObsidianGridField : TextObsidianGridField
    {
        /// <summary>
        /// Delimiter used to join items. Default matches WebForms
        /// <c>ListDelimitedField</c>.
        /// </summary>
        public string Delimiter { get; set; } = ", ";

        /// <inheritdoc/>
        public override object TransformValue( object rawValue, ObsidianGridFieldContext context )
        {
            if ( rawValue == null )
            {
                return string.Empty;
            }

            if ( rawValue is string )
            {
                // A raw string is already scalar; return as-is.
                return rawValue;
            }

            if ( rawValue is IEnumerable enumerable )
            {
                return string.Join( Delimiter, enumerable.Cast<object>().Where( o => o != null ).Select( o => o.ToString() ) );
            }

            return rawValue.ToString();
        }
    }
}
