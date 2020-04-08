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
    /// Person Anniversary following Event
    /// </summary>
    [Description( "Person Anniversary" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonAnniversary" )]

    [IntegerField( "Lead Days", "The number of days prior to birthday that notification should be sent.", false, 5, "", 0)]
    [IntegerField( "Nth Year", "Only be notified for anniversaries that are a multiple of this number (i.e. a value of 5 will notify you on the person's 5th, 10th, 15th, etc. anniversary).", false, 5, "", 1 )]
    public class PersonAnniversary : EventComponent
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
                    DateTime? nextAnniversary = person.NextAnniversary;
                    if ( person.AnniversaryDate.HasValue && nextAnniversary.HasValue )
                    {
                        int yearMultiplier = GetAttributeValue( followingEvent, "NthYear" ).AsInteger();
                        if ( yearMultiplier == 0 || ( nextAnniversary.Value.Year - person.AnniversaryDate.Value.Year ) % yearMultiplier == 0 )
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

                            if ( ( nextAnniversary.Value.Subtract( processDate ).Days <= leadDays ) &&
                                ( !lastNotified.HasValue || nextAnniversary.Value.Subtract( lastNotified.Value.Date ).Days > leadDays ) )
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion

    }
}
