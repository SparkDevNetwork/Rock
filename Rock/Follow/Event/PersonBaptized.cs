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
    /// Person Baptized following Event
    /// </summary>
    [Description( "Person Baptized" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonBaptized" )]

    [IntegerField( "Max Days Back", "Maximum number of days back to consider", false, 30, "", 0)]
    public class PersonBaptized : EventComponent
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

            if ( followingEvent != null && entity != null )
            {
                var personAlias = entity as PersonAlias;
                if ( personAlias != null && personAlias.Person != null )
                {
                    var person = personAlias.Person;

                    if ( person.Attributes == null )
                    {
                        person.LoadAttributes();
                    }

                    DateTime? baptismDate = person.GetAttributeValue( "BaptismDate" ).AsDateTime();
                    if ( baptismDate.HasValue )
                    {
                        int daysBack = GetAttributeValue( followingEvent, "MaxDaysBack" ).AsInteger();
                        var processDate = RockDateTime.Today;
                        if ( !followingEvent.SendOnWeekends && RockDateTime.Today.DayOfWeek == DayOfWeek.Friday )
                        {
                            daysBack += 2;
                        }

                        if ( processDate.Subtract( baptismDate.Value ).Days < daysBack )
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
