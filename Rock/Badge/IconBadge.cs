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
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge
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
        [RockObsolete( "1.10" )]
        [Obsolete( "This method will be removed, use the Entity param instead.", false )]
        public virtual string GetToolTipText( Person person ) {
            return string.Empty;
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This method will be removed, use the Entity param instead.", false )]
        public virtual string GetIconPath( Person person )
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual string GetToolTipText( IEntity entity )
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual string GetIconPath( IEntity entity )
        {
            return string.Empty;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, HtmlTextWriter writer )
        {
            if ( Entity != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "badge" );

                var tooltipText = GetToolTipText( Entity );

                if (tooltipText.IsNullOrWhiteSpace())
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    tooltipText = GetToolTipText( Person );
#pragma warning restore CS0618 // Type or member is obsolete
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Title, tooltipText );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                var iconPath = GetIconPath( Entity );

                if ( iconPath.IsNullOrWhiteSpace() )
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    iconPath = GetIconPath( Person );
#pragma warning restore CS0618 // Type or member is obsolete
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Src, iconPath );
                writer.RenderBeginTag( HtmlTextWriterTag.Img );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }

}