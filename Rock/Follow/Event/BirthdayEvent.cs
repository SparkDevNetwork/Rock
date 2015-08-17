// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock;
using Rock.Data;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

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
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public override bool HasEventHappened( FollowingEventType followingEvent, IEntity entity )
        {
            if ( followingEvent != null && entity != null )
            {
                var personAlias = entity as PersonAlias;
                if ( personAlias != null && personAlias.Person != null )
                {
                    var person = personAlias.Person;
                    if ( person.BirthDay.HasValue && person.BirthMonth.HasValue )
                    {
                        DateTime lastCheck = followingEvent.LastCheckDateTime.HasValue ? followingEvent.LastCheckDateTime.Value : DateTime.MinValue;
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

                        DateTime nextBirthDay = new DateTime( today.Year, person.BirthMonth.Value, person.BirthDay.Value );
                        if ( nextBirthDay.CompareTo( today ) < 0 )
                        {
                            nextBirthDay = nextBirthDay.AddYears( 1 );
                        }

                        if ( ( nextBirthDay.Subtract( processDate ).Days <= leadDays ) &&
                            ( nextBirthDay.Subtract( lastCheck.Date ).Days > leadDays ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override string FormatEntityNotification( FollowingEventType followingEvent, IEntity entity )
        {
            return base.FormatEntityNotification( followingEvent, entity );
        }

        #endregion

    }
}
