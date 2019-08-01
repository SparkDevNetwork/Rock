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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description("Campus Badge")]
    [Export(typeof(BadgeComponent))]
    [ExportMetadata("ComponentName", "Campus")]
    public class Campus : HighlightLabelBadge
    {

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel( IEntity entity )
        {
            if ( CampusCache.All( false ).Count <= 1 || entity == null )
            {
                return null;
            }

            // Campus is associated with the family group(s) person belongs to.
            var families = PersonGroups( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), entity.Id );

            if ( families == null || !families.Any() )
            {
                return null;
            }

            var label = new HighlightLabel();
            label.LabelType = LabelType.Campus;

            var campusNames = new List<string>();
            foreach ( int campusId in families
                .Where( g => g.CampusId.HasValue )
                .Select( g => g.CampusId )
                .Distinct()
                .ToList() )
                campusNames.Add( CampusCache.Get( campusId ).Name );

            label.Text = campusNames.OrderBy( n => n ).ToList().AsDelimited( ", " );

            return label;
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Group> PersonGroups( Guid groupTypeGuid, int personId )
        {
            var groupTypeId = GroupTypeCache.GetId( groupTypeGuid );
            return PersonGroups( groupTypeId ?? 0, personId );
        }

        /// <summary>
        /// The groups of a particular type that current person belongs to
        /// </summary>
        /// <returns></returns>
        public static List<Group> PersonGroups( int groupTypeId, int personId )
        {
            var itemKey = $"RockGroups:{groupTypeId}:{personId}";
            var previouslyQueried = _personGroups.TryGetValue( itemKey, out var groups );

            if ( previouslyQueried )
            {
                return groups;
            }

            var rockContext = new RockContext();
            var service = new GroupMemberService( rockContext );
            groups = service.Queryable()
                .Where( m =>
                    m.PersonId == personId &&
                    m.Group.GroupTypeId == groupTypeId )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                .ThenByDescending( m => m.Group.Name )
                .Select( m => m.Group )
                .OrderByDescending( g => g.Name )
                .ToList();

            _personGroups[itemKey] = groups;
            return groups;
        }

        /// <summary>
        /// Used to cache person groups in case they are queried multiple times
        /// </summary>
        private static Dictionary<string, List<Group>> _personGroups = new Dictionary<string, List<Group>>();
    }
}
