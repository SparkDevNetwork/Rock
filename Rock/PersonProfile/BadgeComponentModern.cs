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
using Rock.Cache;

namespace Rock.PersonProfile
{
    /// <summary>
    /// A new base class for person profile badges. This class was needed in v8.0 to implement a new Render() method. 
    /// The existing Render() method on the base BadgeComponent class could not be updated without causing a breaking change. 
    /// In future releases (after v8), the existing BadgeComponet (with it's obsolete Render() method) can be deleted,
    /// and it's implementation can be moved to this class.
    /// </summary>
    public abstract class BadgeComponentModern : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        [Obsolete( "Use the Render() method that uses a new CachePersonBadge parameter instead.", false )]
        public override void Render( Web.Cache.PersonBadgeCache badge, HtmlTextWriter writer )
        {
            if ( badge == null ) return;
            Render( CachePersonBadge.Get( badge.Id ), writer );
        }

        /// <summary>
        /// Renders the specified badge.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public abstract void Render( CachePersonBadge badge, HtmlTextWriter writer );
    }
}
