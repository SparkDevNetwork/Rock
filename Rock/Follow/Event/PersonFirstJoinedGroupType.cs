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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Follow.Event
{
    /// <summary>
    /// Person First Joined a Group Type following Event
    /// </summary>
    [Description( "Person First Joined Group Type" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonFirstJoinedGroupType" )]

    [GroupTypeField( "Group Type", "The group type to evaluate if person has just joined for the first time", true, order: 0 )]
    [IntegerField( "Max Days Back", "Maximum number of days back to consider", false, 30, "", 1)]
    public class PersonFirstJoinedGroupType : EventComponent
    {
        #region Event Component Implementation

        /// <summary>
        /// Gets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        public override Type FollowedType 
        {
            get { return typeof( Rock.Model.PersonAlias ); }
        }

        /// <summary>
        /// Determines whether [has event happened] [the specified entity].
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="lastNotified">The last notified.</param>
        /// <returns></returns>
        public override bool HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified )
        {
            if ( lastNotified.HasValue )
            {
                return false;
            }

            Guid? groupTypeGuid = GetAttributeValue( followingEvent, "GroupType" ).AsGuidOrNull();
            if ( followingEvent != null && entity != null && groupTypeGuid.HasValue )
            {
                var personAlias = entity as PersonAlias;
                if ( personAlias != null && personAlias.Person != null )
                {
                    var person = personAlias.Person;

                    DateTime? firstJoined = new GroupMemberService( new RockContext() )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.PersonId == personAlias.PersonId &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Group.GroupType.Guid.Equals( groupTypeGuid.Value ) )
                        .Min( m => m.DateTimeAdded );

                    if ( firstJoined.HasValue )
                    {
                        int daysBack = GetAttributeValue( followingEvent, "MaxDaysBack" ).AsInteger();
                        var processDate = RockDateTime.Today;
                        if ( !followingEvent.SendOnWeekends && RockDateTime.Today.DayOfWeek == DayOfWeek.Friday )
                        {
                            daysBack += 2;
                        }

                        if ( processDate.Subtract( firstJoined.Value ).Days < daysBack )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

    }
}
