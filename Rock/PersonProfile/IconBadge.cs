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
using System.Web.UI;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile
{
    /// <summary>
    /// Base class for person profile icon badges
    /// </summary>
    public abstract class IconBadge : BadgeComponent
    {
        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="person">The person.</param>
        public abstract string GetToolTipText(Person person);

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        public abstract string GetIconPath( Person person );

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "badge" );
                writer.AddAttribute( HtmlTextWriterAttribute.Title, GetToolTipText( Person ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Src, GetIconPath( Person ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Img );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }

}