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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description( "Campus Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Campus" )]
    public class Campus : HighlightLabelBadge
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

        /// <summary>
        /// Gets the Entity's Campus badge label even if the campus is inactive.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel( IEntity entity )
        {
            // This badge is only setup to work with a person
            var person = entity as Person;

            // If the entity is not a person or there is only one campus, then don't display a badge
            if ( person == null || CampusCache.All().Count <= 1 )
            {
                return null;
            }

            var campusNames = person.GetCampusIds()
                .Select( id => CampusCache.Get( id )?.Name )
                .Where( name => !name.IsNullOrWhiteSpace() )
                .OrderBy( name => name )
                .ToList();

            if ( !campusNames.Any() )
            {
                return null;
            }

            return new HighlightLabel
            {
                LabelType = LabelType.Campus,
                Text = campusNames.AsDelimited( ", " )
            };
        }
    }
}
