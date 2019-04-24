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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Follow.Event
{
    /// <summary>
    /// Person birthday following Event
    /// </summary>
    [Description( "Person Birthday" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonBirthday" )]

    [IntegerField( "Lead Days", "The number of days prior to birthday that notification should be sent.", false, 5, "", 0)]
    public class PersonBirthday : EventComponent
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
            if ( followingEvent != null && entity != null )
            {
                var personAlias = entity as PersonAlias;
                if ( personAlias != null && personAlias.Person != null )
                {
                    var person = personAlias.Person;
                    DateTime? nextBirthDay = person.NextBirthDay;
                    if ( nextBirthDay.HasValue )
                    {
                        int leadDays = GetAttributeValue( followingEvent, "LeadDays" ).AsInteger();

                        var today = RockDateTime.Today;
                        var processDate = today;
                        if ( !followingEvent.SendOnWeekends )
                        {
                            switch ( today.DayOfWeek )
                            {
                                case DayOfWeek.Friday:
                                    leadDays += 2;
                                    break;
                                case DayOfWeek.Saturday:
                                    processDate = processDate.AddDays( -1 );
                                    leadDays += 2;
                                    break;
                                case DayOfWeek.Sunday:
                                    processDate = processDate.AddDays( -2 );
                                    leadDays += 2;
                                    break;
                            }
                        }

                        if ( ( nextBirthDay.Value.Subtract( processDate ).Days <= leadDays ) &&
                            ( !lastNotified.HasValue || nextBirthDay.Value.Subtract( lastNotified.Value.Date ).Days > leadDays ) )
                        {
                            // If leaddays is greater than zero, ignore any birthdays for today
                            if ( leadDays > 0 && nextBirthDay.Value.Date == RockDateTime.Today )
                            {
                                return false;
                            }
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
