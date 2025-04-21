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
using System.IO;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge
{
    /// <summary>
    /// Base class for person profile icon badges
    /// </summary>
    [RockObsolete( "1.14" )]
    [Obsolete( "HighlightLabelBadge depends on Webforms, use BadgeComponent instead and render the HTML." )]
    public abstract class HighlightLabelBadge : BadgeComponent
    {
        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="entity">The person.</param>
        /// <returns></returns>
        public virtual HighlightLabel GetLabel( IEntity entity )
        {
            return null;
        }

        /// <summary>
        /// Renders the badge HTML content that should be inserted into the DOM.
        /// </summary>
        /// <param name="badge">The badge cache that describes this badge.</param>
        /// <param name="entity">The entity to render the badge for.</param>
        /// <param name="writer">The writer to render the HTML into.</param>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( entity == null )
            {
                return;
            }

            using ( var htmlWriter = new System.Web.UI.HtmlTextWriter( writer ) )
            {
                // Code using the old Person interface will return null here. The else block handles rendering for those obsolete badges
                var label = GetLabel( entity );

                if ( label != null )
                {
                    label.RenderControl( htmlWriter );
                }
                else
                {
                    label = GetLabel( entity as Person );

                    if ( label != null )
                    {
                        label.RenderControl( htmlWriter );
                    }
                }
            }
        }
    }
}
