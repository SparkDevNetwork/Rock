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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Rock.Data;

#if REVIEW_NET5_0_OR_GREATER
using DbEntityEntry = Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry;
#endif

namespace Rock.Model
{
    public partial class EventItemOccurrence
    {
        #region Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The database entity entry.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            // Set the NextDateTime field to the datetime of the next occurrence, or null if there is no future occurrence.
            if ( entry.State == EntityState.Added
                 || entry.State == EntityState.Modified )
            {
                var rockContext = ( RockContext ) dbContext;
                DateTime? nextDate = null;

                // If the event has a schedule and both the schedule and the event are active, get the next occurrence date.
                if ( ScheduleId != null )
                {
                    if ( Schedule == null )
                    {
                        var scheduleService = new ScheduleService( rockContext );
                        Schedule = scheduleService.Get( ScheduleId.Value );
                    }
                    if ( Schedule != null && Schedule.IsActive )
                    {
                        if ( EventItem == null )
                        {
                            var eventService = new EventItemService( rockContext );
                            EventItem = eventService.Get( EventItemId );
                        }
                        if ( EventItem.IsActive )
                        {
                            nextDate = Schedule.GetNextStartDateTime( RockDateTime.Now );
                        }
                    }
                }

                entry.CurrentValues[nameof( NextStartDateTime )] = nextDate;
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Gets the start times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetStartTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            if ( Schedule != null )
            {
                return Schedule.GetScheduledStartTimes( beginDateTime, endDateTime );
            }
            else
            {
                return new List<DateTime>();
            }
        }

        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetFirstStartDateTime()
        {
            if ( Schedule != null )
            {
                return Schedule.GetFirstStartDateTime();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.EventItem != null )
            {
                return string.Format( "{0} ({1})", this.EventItem.Name,
                    this.Campus != null ? this.Campus.Name : "All Campuses" );
            }

            return base.ToString();
        }

        #endregion
    }
}
