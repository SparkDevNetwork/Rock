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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge
{
    /// <summary>
    /// Base class for person profile icon badges
    /// </summary>
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
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the IEntity param instead.", false )]
        public virtual HighlightLabel GetLabel( Person person )
        {
            return null;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Entity != null )
            {
                // Code using the old Person interface will return null here. The else block handles rendering for those obsolete badges
                var label = GetLabel( Entity );
                
                if ( label != null )
                {
                    label.RenderControl( writer );
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    label = GetLabel( Person );
#pragma warning restore CS0618 // Type or member is obsolete

                    if ( label != null )
                    {
                        label.RenderControl( writer );
                    }
                }
            }
        }
    }

}