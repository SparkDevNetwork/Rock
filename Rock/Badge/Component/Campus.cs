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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description( "Campus Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Campus" )]
    [Rock.SystemGuid.EntityTypeGuid( "D4B2BA9B-4F2C-47CB-A5BB-F3FF53A68F39")]
    public class Campus : BadgeComponent
    {
        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            // This badge is only setup to work with a person.
            if ( !( entity is Person person ) )
            {
                return;
            }

            // If there is only one campus, then don't display a badge.
            if ( CampusCache.All().Count <= 1 )
            {
                return;
            }

            var campusNames = person.GetCampusIds()
                .Select( id => CampusCache.Get( id )?.Name )
                .Where( name => !name.IsNullOrWhiteSpace() )
                .OrderBy( name => name )
                .Select( name => name.EncodeHtml() )
                .ToList();

            if ( !campusNames.Any() )
            {
                return;
            }

            writer.Write( $"<div class=\"rockbadge rockbadge-label\" title=\"Campus\"><span class=\"label label-campus\">{campusNames.AsDelimited( "," )}</span></div>" );
        }
    }
}
