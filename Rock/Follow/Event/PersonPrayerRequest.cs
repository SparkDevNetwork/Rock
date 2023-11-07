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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Follow.Event
{
    /// <summary>
    /// Person Baptized following Event
    /// </summary>
    [Description( "Person Prayer Request" )]
    [Export( typeof( EventComponent ) )]
    [ExportMetadata( "ComponentName", "PersonPrayerRequest" )]

    [IntegerField( "Max Days Back", "Maximum number of days back to consider", false, 30, "", 0)]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PERSON_PRAYER_REQUEST )]
    public class PersonPrayerRequest : EventComponent
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
                int daysBack = GetAttributeValue( followingEvent, "MaxDaysBack" ).AsInteger();
                var processDate = RockDateTime.Today;

                // This is DayOfWeek.Monday vs RockDateTime.FirstDayOfWeek because it including stuff that happened on the weekend (Saturday and Sunday) when it the first Non-Weekend Day (Monday)
                if ( !followingEvent.SendOnWeekends && processDate.DayOfWeek == DayOfWeek.Monday ) 
                {
                    daysBack += 2;
                }

                var cutoffDateTime = processDate.AddDays( 0 - daysBack );
                if ( lastNotified.HasValue && lastNotified.Value > cutoffDateTime )
                {
                    cutoffDateTime = lastNotified.Value;
                }

                var personAlias = entity as PersonAlias;
                if ( personAlias != null && personAlias.Person != null )
                {
                    if ( followingEvent.IncludeNonPublicRequests )
                    {
                        return HasPublicOrPrivatePrayerRequest( personAlias, cutoffDateTime );
                    }
                    else
                    {
                        return HasPublicPrayerRequest( personAlias, cutoffDateTime );
                    }
                }
            }

            return false;
        }

        #endregion

        private bool HasPublicPrayerRequest( PersonAlias personAlias, DateTime cutoffDateTime )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PrayerRequestService( rockContext )
                    .Queryable().AsNoTracking()
                    .Any( r =>
                        r.RequestedByPersonAlias != null &&
                        r.RequestedByPersonAlias.PersonId == personAlias.PersonId &&
                        r.ApprovedOnDateTime.HasValue &&
                        r.ApprovedOnDateTime.Value >= cutoffDateTime && 
                        r.IsApproved == true &&
                        r.IsPublic == true );
            }
        }

        private bool HasPublicOrPrivatePrayerRequest( PersonAlias personAlias, DateTime cutoffDateTime )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PrayerRequestService( rockContext )
                    .Queryable().AsNoTracking()
                    .Any( r =>
                        r.RequestedByPersonAlias != null &&
                        r.RequestedByPersonAlias.PersonId == personAlias.PersonId &&
                        r.ApprovedOnDateTime.HasValue &&
                        r.ApprovedOnDateTime.Value >= cutoffDateTime && 
                        r.IsApproved == true );
            }
        }
    }
}
