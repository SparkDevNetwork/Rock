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
using System.Linq;
using System.Web;

namespace Rock.Obsidian.UI.GridField
{
    /// <summary>
    /// Value-shaping subclass that renders a phone number as a <c>tel:</c> link
    /// via server-rendered HTML. Restores the click-to-dial affordance
    /// PhoneNumberSelect had in WebForms without depending on the inline
    /// javascript: PBX handler (which does not survive an Obsidian SPA).
    /// </summary>
    public class PhoneObsidianGridField : HtmlObsidianGridField
    {
        /// <inheritdoc/>
        public override object TransformValue( object rawValue, ObsidianGridFieldContext context )
        {
            if ( rawValue == null )
            {
                return string.Empty;
            }

            var formatted = rawValue.ToString();
            if ( string.IsNullOrWhiteSpace( formatted ) )
            {
                return string.Empty;
            }

            /*
                2026-08-12 - DH

                tel: expects a bare-digit or +digit sequence. Strip anything that is
                not a digit or leading + so the link is dialable regardless of what
                pretty formatting the raw value carries.

                Reason: WebForms used an inline javascript: PBX handler which does not
                survive the SPA; a tel: link is the platform-native equivalent.
            */
            var dialable = new string( formatted.Where( c => char.IsDigit( c ) || c == '+' ).ToArray() );

            return $"<a href=\"tel:{HttpUtility.HtmlEncode( dialable )}\">{HttpUtility.HtmlEncode( formatted )}</a>";
        }
    }
}
